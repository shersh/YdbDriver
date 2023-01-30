using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;

namespace Yandex.Ydb.Driver;

internal sealed class YdbConnector : IYdbConnector, IAsyncDisposable
{
    private GrpcChannel? _channel;
    private CallInvoker _invoker;
    private readonly CallOptions _defaultOptions;

    internal YdbConnector(YdbDataSource dataSource)
    {
        DataSource = dataSource;
        _defaultOptions = new CallOptions()
        {
            Headers = { }
        };
    }

    internal YdbDataSource DataSource { get; }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.ShutdownAsync();
            _channel.Dispose();
        }
    }

    internal async Task Open(TimeSpan timeout, CancellationToken token)
    {
        Debug.Assert(_channel == null);

        var settings = DataSource.Settings;
        _channel = GrpcChannel.ForAddress($"http://{settings.Host}:{settings.Port}", new GrpcChannelOptions()
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                MaxConnectionsPerServer = 1000
            }
        });
        await _channel.ConnectAsync(token);

        _invoker = _channel.CreateCallInvoker();
    }

    public async ValueTask<TResponse> UnaryCallAsync<TRequest, TResponse>(Method<TRequest, TResponse> method,
        TRequest request, CallOptions? options = null) where TRequest : class where TResponse : class
    {
        return await _invoker.AsyncUnaryCall(method, null, options ?? GetDefaultOptions(), request);
    }

    private CallOptions GetDefaultOptions()
    {
        return _defaultOptions;
    }

    public TResponse UnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
        TRequest request, CallOptions? options = null) where TRequest : class where TResponse : class
    {
        return _invoker.BlockingUnaryCall(method, null, options ?? GetDefaultOptions(), request);
    }
}