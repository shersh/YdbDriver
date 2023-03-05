using System.Data.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Yandex.Ydb.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class YdbServiceCollectionExtensions
{
    /// <summary>
    ///     Registers an <see cref="YdbDataSource" /> and an <see cref="YdbConnection" /> in the
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An Ydb connection string.</param>
    /// <param name="connectionLifetime">
    ///     The lifetime with which to register the <see cref="YdbConnection" /> in the container.
    ///     Defaults to <see cref="ServiceLifetime.Scoped" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    ///     The lifetime with which to register the <see cref="YdbDataSource" /> service in the container.
    ///     Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddYdbDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
    {
        return AddYdbDataSourceCore(
            serviceCollection, connectionString, null, connectionLifetime, dataSourceLifetime);
    }

    public static IServiceCollection AddYdbDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<YdbDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
    {
        return AddYdbDataSourceCore(serviceCollection, connectionString, dataSourceBuilderAction, connectionLifetime,
            dataSourceLifetime);
    }


    /// <summary>
    ///     Registers an <see cref="YdbDataSource" /> and an <see cref="YdbConnection" /> in the
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An Ydb connection string.</param>
    /// <param name="dataSourceBuilderAction">
    ///     An action to configure the <see cref="YdbDataSourceBuilder" /> for further customizations of the
    ///     <see cref="YdbDataSource" />.
    /// </param>
    /// <param name="connectionLifetime">
    ///     The lifetime with which to register the <see cref="YdbConnection" /> in the container.
    ///     Defaults to <see cref="ServiceLifetime.Scoped" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    ///     The lifetime with which to register the <see cref="YdbDataSource" /> service in the container.
    ///     Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    private static IServiceCollection AddYdbDataSourceCore(IServiceCollection serviceCollection,
        string connectionString, Action<YdbDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime, ServiceLifetime dataSourceLifetime)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(YdbDataSource),
                sp =>
                {
                    var dataSourceBuilder = new YdbDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(dataSourceBuilder);
                    return dataSourceBuilder.Build();
                },
                dataSourceLifetime));

        AddCommonServices(serviceCollection, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    private static void AddCommonServices(
        IServiceCollection serviceCollection,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(YdbConnection),
                sp => sp.GetRequiredService<YdbDataSource>().CreateConnection(),
                connectionLifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(DbDataSource),
                sp => sp.GetRequiredService<YdbDataSource>(),
                dataSourceLifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(DbConnection),
                sp => sp.GetRequiredService<YdbConnection>(),
                connectionLifetime));
    }
}