using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;
using Ydb.Operations;
using Ydb.Table;

namespace Yandex.Ydb.Driver;

public interface IRetryPolicyManager
{
    AsyncPolicy<Operation> GetDefaultOperationPolicy();
}

internal sealed class RetryPolicyManager : IRetryPolicyManager
{
    public ILogger Logger { get; }

    private static readonly HashSet<StatusIds.Types.StatusCode> RetryableCodes =
        new HashSet<StatusIds.Types.StatusCode>()
        {
            StatusIds.Types.StatusCode.SessionBusy,
            StatusIds.Types.StatusCode.Overloaded,
            StatusIds.Types.StatusCode.InternalError,
            StatusIds.Types.StatusCode.Timeout
        };

    private readonly Lazy<AsyncRetryPolicy<Operation>> _defaultPolicy;

    public RetryPolicyManager(ILogger logger)
    {
        Logger = logger;
        _defaultPolicy = new Lazy<AsyncRetryPolicy<Operation>>(() =>
        {
            return Policy
                .HandleResult<Operation>(operation =>
                    RetryableCodes.Contains(operation.Status))
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt)),
                    (result, span, retryAttempt, _) =>
                    {
                        LogMessages.WaitAndRetryOperation(Logger, result.Result.Id, retryAttempt, span,
                            result.Result.Status);
                        return Task.CompletedTask;
                    });
        });
    }

    public AsyncPolicy<Operation> GetDefaultOperationPolicy()
    {
        return _defaultPolicy.Value;
    }
}

public sealed class YdbConnection : DbConnection
{
    private readonly YdbDataSource _dataSource;
    private YdbConnectionState _connectionState;
    private string _databaseName;

    private ILogger _logger;
    private string _sessionId;
    private ConnectionState _state;

    public YdbConnection()
    {
        GC.SuppressFinalize(this);
    }

    public YdbConnection(string? connectionString) : this()
    {
        ConnectionString = connectionString;
    }

    private YdbConnection(YdbDataSource dataSource) : this(dataSource.ConnectionString)
    {
        _dataSource = dataSource;
        _databaseName = _dataSource.Settings.Database;
    }

    internal YdbConnector? Connector { get; private set; }

    internal IRetryPolicyManager RetryPolicyManager => _dataSource.Configuration.RetryPolicyManager;

    [AllowNull]
    public override string ConnectionString
    {
        get => "";
        set =>
            _connectionState =
                new YdbConnectionState(_state, new YdbConnectionSettings(new YDbConnectionStringBuilder(value)));
    }

    public override string Database => _databaseName;
    public override ConnectionState State => _state;

    public override string DataSource => string.Empty;

    public override string ServerVersion => "1.0";
    public TypeMapper TypeMapper => _dataSource.TypeMapper;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        var transaction = new YdbTransaction(this);
        transaction.Init(isolationLevel);
        return transaction;
    }

    public override void ChangeDatabase(string databaseName)
    {
        _databaseName = databaseName;
        //TODO: Here we need to change session
    }

    public override async ValueTask DisposeAsync()
    {
        await CloseAsync();
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        Close();
        base.Dispose(disposing);
    }

    public override async Task CloseAsync()
    {
        _state = ConnectionState.Closed;

        if (!string.IsNullOrEmpty(_sessionId)) await _dataSource.ReturnAsync(_sessionId);

        if (Connector is not null) Connector = null;
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;

        if (!string.IsNullOrEmpty(_sessionId)) _dataSource.Return(_sessionId);

        if (Connector is not null) Connector = null;
    }

    public override void Open()
    {
        OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task OpenAsync(CancellationToken ctx)
    {
        if (State is ConnectionState.Connecting
            or ConnectionState.Open
            or ConnectionState.Fetching
            or ConnectionState.Executing)
            return;

        Debug.Assert(Connector is null, "Connector == null");

        if (_dataSource is null)
        {
            Debug.Assert(string.IsNullOrEmpty(ConnectionString));
            throw new InvalidOperationException("The ConnectionString property has not been initialized.");
        }

        _logger = _dataSource.LoggingConfiguration.ConnectionLogger;
        LogMessages.OpeningConnection(_logger, _connectionState.Settings.Host, _connectionState.Settings.Port,
            _databaseName);

        try
        {
            var connector = await _dataSource.Get(this, TimeSpan.FromSeconds(ConnectionTimeout), ctx);
            Connector = connector;
            _sessionId = await _dataSource.GetSession(Database);
            _state = ConnectionState.Open;
        }
        catch (Exception)
        {
            _state = ConnectionState.Closed;
            throw;
        }
    }

    internal string GetSessionId()
    {
        return _sessionId;
    }

    protected override DbCommand CreateDbCommand()
    {
        return CreateYdbCommand();
    }

    public YdbCommand CreateYdbCommand(string? yql = default)
    {
        return new YdbCommand(this) { CommandText = yql };
    }

    internal static YdbConnection FromDataSource(YdbDataSource dataSource)
    {
        return new YdbConnection(dataSource);
    }

    internal ILogger GetCommandLogger()
    {
        return _dataSource.Configuration.LoggingConfiguration.CommandLogger;
    }
}