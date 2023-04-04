using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Credentials;

namespace Yandex.Ydb.Driver;

internal interface IYdbConnectorProvider
{
    YdbConnector GetConnector(string? preferedEndpoint);

    void PessimizeConnector(YdbConnector connector);

    void DrainConnector(YdbConnector connector);
}

public sealed class NoConnectorsException : YdbDriverException
{
}

internal class YdbConnectionProvider : IYdbConnectorProvider
{
    private class ConnectorsCache
    {
        private readonly List<string> _endpoints = new();
        private readonly Dictionary<string, YdbConnector> _connectors = new();

        internal YdbConnector? GetByEndpoint(string endpoint) =>
            _connectors.TryGetValue(endpoint, out var res) ? res : null;

        internal YdbConnector? GetRandom() => _endpoints.Count == 0
            ? null
            : _connectors[_endpoints[Random.Shared.Next(0, _endpoints.Count)]];

        internal void Remove(YdbConnector connector)
        {
            _endpoints.Remove(connector.Endpoint);
            _connectors.Remove(connector.Endpoint);
        }

        internal void Clear()
        {
            _endpoints.Clear();
            _connectors.Clear();
        }

        internal void Add(YdbConnector connector)
        {
            if (_connectors.ContainsKey(connector.Endpoint))
                return;

            _endpoints.Add(connector.Endpoint);
            _connectors[connector.Endpoint] = connector;
        }

        internal int Count => _endpoints.Count;
    }

    private readonly ILogger _logger;
    private readonly YdbConnectionSettings _settings;
    private readonly ICredentialsProvider _credentialsProvider;

    private readonly ConnectorsCache _active = new();
    private readonly ConnectorsCache _passive = new();

    private readonly object _lckObj = new();
    private readonly Lazy<Task<YdbConnector>> _getInitialConnector;
    private readonly CancellationTokenSource _cts;

    public YdbConnectionProvider(ILogger logger, YdbConnectionSettings settings,
        ICredentialsProvider credentialsProvider)
    {
        _logger = logger;
        _settings = settings;
        _credentialsProvider = credentialsProvider;

        _cts = new CancellationTokenSource();

        _getInitialConnector = new Lazy<Task<YdbConnector>>(async () =>
        {
            var connector =
                CreateConnector($"{(_settings.UseSsl ? "https" : "http")}://{_settings.Host}:{_settings.Port}");
            await connector.Open(TimeSpan.MaxValue, CancellationToken.None);
            return connector;
        });

        Task.Factory.StartNew(RefreshAsync, _cts.Token, TaskCreationOptions.LongRunning);
    }

    private async Task RefreshAsync(object? state)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(_cts.Token);
                var connector = await _getInitialConnector.Value;
                connector.UnaryCallAsync();
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to refresh endpoints");
            }
        }
    }

    private YdbConnector CreateConnector(string endpoint)
    {
        //$"{(_settings.UseSsl ? "https" : "http")}://{_settings.Host}:{_settings.Port}"
        return new YdbConnector(_logger, endpoint,
            _settings, _credentialsProvider);
    }

    public YdbConnector GetConnector(string? preferredEndpoint)
    {
        lock (_lckObj)
        {
            YdbConnector? connector;
            if (preferredEndpoint != null)
            {
                connector = _active.GetByEndpoint(preferredEndpoint);
                if (connector != null)
                    return connector;
            }

            connector = _active.GetRandom();
            if (connector != null)
                return connector;

            connector = _passive.GetRandom();
            if (connector != null)
                return connector;

            throw new NoConnectorsException();
        }
    }

    public void PessimizeConnector(YdbConnector connector)
    {
        lock (_lckObj)
        {
            _active.Remove(connector);
            _passive.Add(connector);

            var sum = _active.Count + _passive.Count;
            var ratio = sum > 0 ? (double)_passive.Count / sum : 1;
            LogPessimazeConnector(connector.Endpoint, ratio);
        }
    }

    public void DrainConnector(YdbConnector connector)
    {
        lock (_lckObj)
        {
            _active.Remove(connector);
            _passive.Remove(connector);
        }
    }

    [LoggerMessage(Message = "Pessimized connection to endpoint `endpoint` with ratio `{ratio}`")]
    public partial void LogPessimazeConnector(string endpoint, double ratio);
}

internal sealed class PooledDataSource : YdbDataSource
{
    private readonly List<IAsyncDisposable> _disposables = new();
    private YdbConnector _connector;

    private long _currentSessions;
    private bool _isBootstrapped;
    private ISessionPool _sessionPool;

    internal PooledDataSource(YDbConnectionStringBuilder connectionStringBuilder, YdbDataSourceConfiguration config) :
        base(connectionStringBuilder, config)
    {
    }

    public override (long, long, long) Statistics =>
        (Interlocked.Read(ref _currentSessions), 0, 0);

    internal override async ValueTask<YdbConnector> Get(YdbConnection conn, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (_isBootstrapped)
            return _connector;

        await Bootstrap();
        return _connector;
    }

    internal override async ValueTask Bootstrap()
    {
        if (_isBootstrapped)
            return;

        _isBootstrapped = true;
        _connector = await OpenNewConnector(TimeSpan.FromSeconds(60), CancellationToken.None);
        _sessionPool = new SessionPool(Configuration.LoggingConfiguration.SessionLogger, _connector,
            Settings.MaxSessions);
        await _sessionPool.Initialize(Settings.Database);

        await base.Bootstrap();
    }

    public override async ValueTask<string> GetSession(string database)
    {
        return await _sessionPool.GetSession(database);
    }

    internal override ValueTask ReturnAsync(string session)
    {
        _sessionPool.Return(session);
        return ValueTask.CompletedTask;
    }

    internal override void Return(string session)
    {
        _sessionPool.Return(session);
    }

    private async ValueTask<YdbConnector> OpenNewConnector(TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var connector = new YdbConnector(this);
        _disposables.Add(connector);

        await connector.Open(timeout, cancellationToken);
        return connector;
    }

    protected override async ValueTask ClearAsync()
    {
        foreach (var disposable in _disposables)
            await disposable.DisposeAsync();

        _disposables.Clear();
    }

    protected override void Clear()
    {
        _sessionPool?.Dispose();
        ClearAsync().GetAwaiter().GetResult();
    }

    internal override bool TryGetIdleConnector(out YdbConnector connector)
    {
        connector = _connector;
        return true;
    }
}