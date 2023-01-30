using System.Collections.Concurrent;

namespace Yandex.Ydb.Driver;

internal sealed class PooledDataSource : YdbDataSource
{
    private readonly List<IAsyncDisposable> _disposables = new();
    private bool _isBootstrapped = false;
    private YdbConnector _connector;
    private ISessionPool _sessionPool;

    internal PooledDataSource(YDbConnectionStringBuilder connectionStringBuilder, YdbDataSourceConfiguration config) :
        base(connectionStringBuilder, config)
    {
    }

    internal override async ValueTask<YdbConnector> Get(YdbConnection conn, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (_isBootstrapped)
            return _connector;

        await Bootstrap();
        return _connector;
    }

    public override (long, long, long) Statistics =>
        (Interlocked.Read(ref _currentSessions), 0, 0);

    private long _currentSessions;

    internal override async ValueTask Bootstrap()
    {
        if (_isBootstrapped)
            return;

        _isBootstrapped = true;
        _connector = await OpenNewConnector(TimeSpan.FromSeconds(60), CancellationToken.None);
        _sessionPool = new SessionPool(_connector, this.Settings.MaxSessions);
        await _sessionPool.Initialize(Settings.Database);

        await base.Bootstrap();
    }

    public override async ValueTask<string> GetSession(string database)
    {
        return await _sessionPool.GetSession(database);
    }

    internal override async ValueTask ReturnAsync(string session)
    {
        _sessionPool.Return(session);
        return;
    }

    internal override void Return(string session)
    {
        _sessionPool.Return(session);
        return;
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

    internal override bool TryGetIdleConnector(out YdbConnector? connector)
    {
        connector = _connector;
        return true;
    }
}