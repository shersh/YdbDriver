using Yandex.Cloud.Credentials;
using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver;

internal record YdbDataSourceConfiguration(YdbLoggingConfiguration LoggingConfiguration,
    List<TypeHandlerResolverFactory> ResolverFactories,
    Dictionary<string, IUserTypeMapping> UserTypeMappings, ICredentialsProvider CredentialsProvider,
    IRetryPolicyManager RetryPolicyManager)
{
}