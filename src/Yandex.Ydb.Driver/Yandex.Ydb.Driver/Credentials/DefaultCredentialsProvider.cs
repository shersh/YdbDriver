using Yandex.Cloud.Credentials;

namespace Yandex.Ydb.Driver.Credentials;

public class DefaultCredentialsProvider : ICredentialsProvider
{
    public string GetToken() => string.Empty;
}