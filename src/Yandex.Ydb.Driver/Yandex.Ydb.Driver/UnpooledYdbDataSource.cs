using Grpc.Core;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb.Table;
using Ydb.Table.V1;

namespace Yandex.Ydb.Driver;

/// <inheritdoc />
internal sealed class UnpooledYdbDataSource : YdbDataSource
{
    private readonly List<IAsyncDisposable> _disposables = new();
    private YdbConnector _connector;
    private bool _isBootstrapped;

    /// <inheritdoc />
    public UnpooledYdbDataSource(YDbConnectionStringBuilder settings, YdbDataSourceConfiguration config) : base(
        settings, config)
    {
    }

    internal override async ValueTask<YdbConnector> Get(YdbConnection conn, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        await Bootstrap();
        return _connector;
    }

    internal override async ValueTask Bootstrap()
    {
        if (_isBootstrapped)
            return;

        _isBootstrapped = true;

        _connector = await OpenNewConnector(TimeSpan.FromSeconds(60), CancellationToken.None);

        await base.Bootstrap();
    }

    protected override async ValueTask ClearAsync()
    {
        foreach (var disposable in _disposables)
            await disposable.DisposeAsync();

        _disposables.Clear();
    }

    protected override void Clear()
    {
        ClearAsync().GetAwaiter().GetResult();
    }

    internal override bool TryGetIdleConnector(out YdbConnector? connector)
    {
        connector = _connector;
        return true;
    }

    public override async ValueTask<string> GetSession(string database)
    {
        try
        {
            var request = new CreateSessionRequest();
            var response = await _connector.UnaryCallAsync(TableService.CreateSessionMethod, request,
                new CallOptions(new Metadata { { YdbMetadata.RpcDatabaseHeader, database } }));
            var result = response.Operation.GetResult<CreateSessionResult>();
            return result.SessionId;
        }
        catch (Exception e)
        {
            throw new YdbDriverException($"Failed to create session. Inner exception: {e}", e);
        }
    }

    internal override void Return(string session)
    {
        _connector.UnaryCall(TableService.DeleteSessionMethod, new DeleteSessionRequest { SessionId = session },
            new CallOptions(new Metadata { { YdbMetadata.RpcDatabaseHeader, Settings.Database } }));
    }

    internal override async ValueTask ReturnAsync(string session)
    {
        await _connector.UnaryCallAsync(TableService.DeleteSessionMethod,
            new DeleteSessionRequest { SessionId = session },
            new CallOptions(new Metadata { { YdbMetadata.RpcDatabaseHeader, Settings.Database } }));
    }

    private async ValueTask<YdbConnector> OpenNewConnector(TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var connector = new YdbConnector(this);
        _disposables.Add(connector);

        await connector.Open(timeout, cancellationToken);
        return connector;
    }
}