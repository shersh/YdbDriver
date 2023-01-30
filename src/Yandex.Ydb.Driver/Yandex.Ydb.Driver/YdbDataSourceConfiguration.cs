using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver;

internal sealed record YdbDataSourceConfiguration(YdbLoggingConfiguration LoggingConfiguration,
    List<TypeHandlerResolverFactory> ResolverFactories, 
    Dictionary<string, IUserTypeMapping> UserTypeMappings
);