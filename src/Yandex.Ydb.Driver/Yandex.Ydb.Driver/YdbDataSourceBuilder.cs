using Microsoft.Extensions.Logging;
using Yandex.Cloud.Credentials;
using Yandex.Ydb.Driver.Credentials;
using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver;

public class YdbDataSourceBuilder : IYdbDataSourceBuilder
{
    private readonly List<TypeHandlerResolverFactory> _resolverFactories = new();
    private readonly Dictionary<string, IUserTypeMapping> _userTypeMappings = new();
    private ILoggerFactory? _loggerFactory;
    private ICredentialsProvider? _provider;

    public YdbDataSourceBuilder(string? connectionString = default)
    {
        ConnectionStringBuilder = new YDbConnectionStringBuilder(connectionString);
        ResetTypeMappings();
    }

    /// <summary>
    ///     A connection string builder that can be used to configured the connection string on the builder.
    /// </summary>
    public YDbConnectionStringBuilder ConnectionStringBuilder { get; }

    /// <summary>
    ///     Returns the connection string, as currently configured on the builder.
    /// </summary>
    public string ConnectionString => ConnectionStringBuilder.ToString();

    /// <summary>
    ///     Sets the <see cref="ILoggerFactory" /> that will be used for logging.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public YdbDataSourceBuilder UseLoggerFactory(ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    public YdbDataSourceBuilder UseCredentials(ICredentialsProvider provider)
    {
        _provider = provider;
        return this;
    }

    public void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory)
    {
        _resolverFactories.Insert(0, resolverFactory);
    }

    private void ResetTypeMappings()
    {
        var globalMapper = GlobalTypeMapper.Instance;
        globalMapper.Lock.EnterReadLock();
        try
        {
            _resolverFactories.Clear();
            foreach (var resolverFactory in globalMapper.ResolverFactories)
                _resolverFactories.Add(resolverFactory);

            _userTypeMappings.Clear();
            foreach (var kv in globalMapper.UserTypeMappings)
                _userTypeMappings[kv.Key] = kv.Value;
        }
        finally
        {
            globalMapper.Lock.ExitReadLock();
        }
    }

    /// <summary>
    ///     Builds and returns an <see cref="NpgsqlDataSource" /> which is ready for use.
    /// </summary>
    public YdbDataSource Build()
    {
        var config = PrepareConfiguration();

        return ConnectionStringBuilder.Pooling
            ? new PooledDataSource(ConnectionStringBuilder, config)
            : new UnpooledYdbDataSource(ConnectionStringBuilder, config);
    }

    private YdbDataSourceConfiguration PrepareConfiguration()
    {
        ConnectionStringBuilder.PostProcessAndValidate();
        return new YdbDataSourceConfiguration(_loggerFactory is null
                ? YdbLoggingConfiguration.NullConfiguration
                : new YdbLoggingConfiguration(_loggerFactory), _resolverFactories, _userTypeMappings,
            _provider ?? new DefaultCredentialsProvider());
    }
}