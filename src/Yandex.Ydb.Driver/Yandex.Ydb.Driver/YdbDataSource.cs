using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Credentials;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver;

/// <inheritdoc />
public abstract class YdbDataSource : DbDataSource
{
    private readonly ILogger _connectionLogger;


    private bool _isBootstrapped;
    private int _isDisposed;

    /// <inheritdoc />
    internal YdbDataSource(YDbConnectionStringBuilder settings, YdbDataSourceConfiguration config)
    {
        Settings = settings;
        ConnectionString = settings.ToString();
        Configuration = config;
        LoggingConfiguration = config.LoggingConfiguration;
        _connectionLogger = LoggingConfiguration.ConnectionLogger;
        CredentialsProvider = config.CredentialsProvider;
        Bootstrap().GetAwaiter().GetResult();
    }

    public virtual (long, long, long) Statistics => (0, 0, 0);

    internal YDbConnectionStringBuilder Settings { get; }

    internal YdbDataSourceConfiguration Configuration { get; }

    internal YdbLoggingConfiguration LoggingConfiguration { get; }

    internal TypeMapper TypeMapper { get; private set; }

    internal ICredentialsProvider CredentialsProvider { get; }

    public override string ConnectionString { get; }

    internal abstract ValueTask<YdbConnector> Get(YdbConnection conn, TimeSpan timeout,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public new YdbConnection OpenConnection()
    {
        LogMessages.CreateDbConnection(LoggingConfiguration.ConnectionLogger);
        var connection = CreateConnection();

        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception e)
        {
            LogMessages.ExceptionToCreateDbConnection(LoggingConfiguration.ConnectionLogger, e);
            connection.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public new async ValueTask<YdbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        LogMessages.CreateDbConnection(LoggingConfiguration.ConnectionLogger);
        var connection = CreateConnection();

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch (Exception e)
        {
            LogMessages.ExceptionToCreateDbConnection(LoggingConfiguration.ConnectionLogger, e);
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    protected override async ValueTask<DbConnection> OpenDbConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override DbConnection CreateDbConnection()
    {
        return CreateConnection();
    }

    /// <summary>
    ///     Creates a command ready for use against this <see cref="YdbDataSource" />.
    /// </summary>
    /// <param name="commandText">An optional SQL for the command.</param>
    public new YdbCommand CreateCommand(string? commandText = null)
    {
        var ydbConnection = OpenConnection();
        return new YdbDataSourceCommand(ydbConnection) { CommandText = commandText! };
    }

    /// <inheritdoc />
    protected override DbCommand CreateDbCommand(string? commandText = null)
    {
        return CreateCommand(commandText);
    }

    /// <inheritdoc />
    protected override DbBatch CreateDbBatch()
    {
        return CreateBatch();
    }

    /// <summary>
    ///     Creates a new <see cref="YdbDataSource" /> for the given <paramref name="connectionString" />.
    /// </summary>
    public static YdbDataSource Create(string connectionString)
    {
        return new YdbDataSourceBuilder(connectionString).Build();
    }

    internal virtual ValueTask Bootstrap()
    {
        if (_isBootstrapped)
            return ValueTask.CompletedTask;

        _isBootstrapped = true;

        var typeMapper = new TypeMapper();
        typeMapper.Initialize(Configuration.ResolverFactories, Configuration.UserTypeMappings, _connectionLogger);

        TypeMapper = typeMapper;

        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Creates a new <see cref="YdbDataSource" /> for the given <paramref name="connectionStringBuilder" />.
    /// </summary>
    public static YdbDataSource Create(YDbConnectionStringBuilder connectionStringBuilder)
    {
        return Create(connectionStringBuilder.ToString());
    }

    /// <inheritdoc />
    protected override DbConnection OpenDbConnection()
    {
        return OpenConnection();
    }

    public new virtual YdbConnection CreateConnection()
    {
        return YdbConnection.FromDataSource(this);
    }

    private void CheckDisposed()
    {
        if (_isDisposed == 1)
            ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
    }

    /// <inheritdoc />
    protected sealed override void Dispose(bool disposing)
    {
        if (disposing && Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            DisposeBase();
    }

    /// <inheritdoc cref="Dispose" />
    private void DisposeBase()
    {
        Clear();
    }

    /// <inheritdoc />
    protected sealed override ValueTask DisposeAsyncCore()
    {
        return Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0 ? DisposeAsyncBase() : default;
    }

    protected virtual async ValueTask DisposeAsyncBase()
    {
        await ClearAsync().ConfigureAwait(false);
    }

    protected abstract ValueTask ClearAsync();

    protected abstract void Clear();

    internal abstract bool TryGetIdleConnector([NotNullWhen(true)] out YdbConnector? connector);
    public abstract ValueTask<string> GetSession(string database);

    internal abstract void Return(string session);
    internal abstract ValueTask ReturnAsync(string session);
}