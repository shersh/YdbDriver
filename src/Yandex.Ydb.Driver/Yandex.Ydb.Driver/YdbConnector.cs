using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Credentials;
using Yandex.Ydb.Driver.Helpers;

namespace Yandex.Ydb.Driver;

internal sealed class YdbConnector : IYdbConnector, IAsyncDisposable
{
    private GrpcChannel? _channel;
    private CallInvoker _invoker;
    private readonly ILogger _logger;
    private readonly YdbConnectionSettings _settings;
    private readonly ICredentialsProvider _credentialsProvider;
    internal string Endpoint { get; }

    internal YdbConnector(ILogger logger, string endpoint, YdbConnectionSettings settings,
        ICredentialsProvider credentialsProvider)
    {
        _logger = logger;
        _settings = settings;
        _credentialsProvider = credentialsProvider;
        Endpoint = endpoint;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            LogGrpcShutdown(Endpoint);
            await _channel.ShutdownAsync();
            _channel.Dispose();
        }
    }

    internal async Task Open(TimeSpan timeout, CancellationToken token)
    {
        Debug.Assert(_channel == null);

        var handler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            MaxConnectionsPerServer = 1000
        };

        if (_settings.UseSsl)
        {
            var path = _settings.RootCertificate;
            if (path != null)
            {
                if (!File.Exists(Path.Combine(path, "cert.pem")))
                    ThrowHelper.FileNotFound($"Cert.pem file does not exist in path `{path}`", "cert.pem");

                if (!File.Exists(Path.Combine(path, "key.pem")))
                    ThrowHelper.FileNotFound($"Key.pem file does not exist in path `{path}`", "key.pem");

                var cert = X509Certificate2.CreateFromPemFile(Path.Combine(path, "cert.pem"),
                    Path.Combine(path, "key.pem"));

                handler.SslOptions.ClientCertificates ??= new X509CertificateCollection();
                handler.SslOptions.ClientCertificates.Add(cert);

                if (_settings.TrustSsl)
                    handler.SslOptions.CertificateChainPolicy = new X509ChainPolicy
                        { TrustMode = X509ChainTrustMode.CustomRootTrust, CustomTrustStore = { cert } };
            }
        }

        _channel = GrpcChannel.ForAddress(Endpoint, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        LogGrpcConnecting(Endpoint);
        await _channel.ConnectAsync(token);

        _invoker = _channel.CreateCallInvoker();
    }

    private CallOptions PopulateHeaders(CallOptions options)
    {
        var token = _credentialsProvider.GetToken();
        var headers = options.Headers ?? new Metadata();
        if (!string.IsNullOrEmpty(token) && headers.All(x => x.Key != YdbMetadata.RpcAuthHeader))
            headers.Add(YdbMetadata.RpcAuthHeader, token);

        if (headers.All(x => x.Key != YdbMetadata.RpcDatabaseHeader))
            headers.Add(YdbMetadata.RpcDatabaseHeader, _settings.Database);

        return options.WithHeaders(headers);
    }

    public async ValueTask<TResponse> UnaryCallAsync<TRequest, TResponse>(Method<TRequest, TResponse> method,
        TRequest request, CallOptions? options = null) where TRequest : class where TResponse : class
    {
        var callOptions = PopulateHeaders(options ?? GetDefaultOptions());
        return await _invoker.AsyncUnaryCall(method, null, callOptions, request);
    }

    private CallOptions GetDefaultOptions()
    {
        return new CallOptions();
    }

    public TResponse UnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
        TRequest request, CallOptions? options = null) where TRequest : class where TResponse : class
    {
        var callOptions = PopulateHeaders(options ?? GetDefaultOptions());
        return _invoker.BlockingUnaryCall(method, null, callOptions, request);
    }

    #region LOGGING

    [LoggerMessage(Message = "Connecting to grpc endpoint `{endpoint}`", Level = LogLevel.Information)]
    public partial void LogGrpcConnecting(string endpoint);

    [LoggerMessage(Message = "Shutdown connection to grpc endpoint `{endpoint}`", Level = LogLevel.Information)]
    public partial void LogGrpcShutdown(string endpoint);

    #endregion
}