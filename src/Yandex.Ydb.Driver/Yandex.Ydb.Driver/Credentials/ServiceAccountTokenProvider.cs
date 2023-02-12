using System.Text.Json;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.SDK.IamJwtCredentialsProviderExtension;

namespace Yandex.Ydb.Driver.Credentials;

public class ServiceAccountTokenProvider : ICredentialsProvider
{
    //TODO: Recreate own JwtCredProvider if issue will not be resolved, see https://github.com/RVShershnev/Yandex.Cloud.SDK.IamJwtCredentialsProviderExtension/issues/1
    private readonly IamJwtCredentialsProvider _provider;

    public ServiceAccountTokenProvider(string serviceAccountId, string keyId, string pemCert)
    {
        _provider = new IamJwtCredentialsProvider(serviceAccountId, keyId, pemCert);
    }

    /// <summary>
    /// Constructs new ServiceAccountTokenProvider
    /// </summary>
    /// <param name="pathToFile">Full path to .json file with private and public keys</param>
    public ServiceAccountTokenProvider(string pathToFile)
    {
        var reader = File.OpenText(pathToFile);
        var keyJson = JsonDocument.Parse(reader.ReadToEnd());
        _provider = new IamJwtCredentialsProvider(
            keyJson.RootElement.GetProperty("service_account_id").GetString()!,
            keyJson.RootElement.GetProperty("id").GetString()!,
            keyJson.RootElement.GetProperty("private_key").GetString()!
        );
    }

    public string GetToken()
    {
        return _provider.GetToken();
    }
}