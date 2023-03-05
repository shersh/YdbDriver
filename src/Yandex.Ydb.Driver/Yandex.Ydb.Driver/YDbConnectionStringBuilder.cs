using System.Data.Common;
using System.Globalization;
using Yandex.Ydb.Driver.Helpers;

namespace Yandex.Ydb.Driver;

public class YDbConnectionStringBuilder : DbConnectionStringBuilder
{
    /// <summary>
    ///     The default GRPC port of server
    /// </summary>
    public const ushort DefaultGrpcPort = 2136;

    /// <summary>
    ///     The default GRPC port of server
    /// </summary>
    public const ushort DefaultTlsGrpcPort = 2135;

    /// <summary>
    ///     The default GRPC port of server
    /// </summary>
    public const string DefaultSchema = "grpc";


    public const string DefaultDatabase = "local";

    /// <summary>
    ///     The default command timeout in <b>seconds</b> (15).
    /// </summary>
    public const int DefaultCommandTimeout = 15;

    /// <summary>
    ///     The default name of the client ('Yandex.YdbDriverClient').
    /// </summary>
    public const string DefaultClientName = "Yandex.YdbDriverClient";

    public YDbConnectionStringBuilder(string? connectionString)
    {
        ConnectionString = connectionString;
    }

    public YDbConnectionStringBuilder(YdbConnectionSettings? settings)
    {
        if (settings == null)
            ThrowHelper.ThrowNullException(nameof(settings));

        Host = settings.Host;
        Port = settings.Port;
        Database = settings.Database;
        User = settings.User;
        Password = settings.Password;
    }

    /// <summary>
    ///     Gets or sets the name or the IP address of the host.
    /// </summary>
    /// <returns>The name or the IP address of the host.</returns>
    public string? Host
    {
        get => GetStringOrDefault(nameof(Host), "localhost");
        set => this[nameof(Host)] = value;
    }

    public string Database
    {
        get => GetStringOrDefault(nameof(Database), DefaultDatabase);
        set => this[nameof(Database)] = value;
    }

    /// <summary>
    ///     Gets or sets the IP port of the server.
    /// </summary>
    /// <returns>The IP port of the server. The default value is <see cref="DefaultPort" />.</returns>
    public ushort Port
    {
        get => (ushort)GetInt32OrDefault(nameof(Port), DefaultGrpcPort);
        set => this[nameof(Port)] = value;
    }

    /// <summary>
    ///     Gets or sets the name of the user.
    /// </summary>
    /// <returns>The name of the user. The default value is <see cref="DefaultUser" />.</returns>
    public string? User
    {
        get => GetString(nameof(User));
        set => this[nameof(User)] = value;
    }

    /// <summary>
    ///     Gets or sets the password.
    /// </summary>
    /// <returns>The password.</returns>
    public string? Password
    {
        get => GetString(nameof(Password));
        set => this[nameof(Password)] = value;
    }

    /// <summary>
    ///     Gets or sets the path to the file that contains a certificate (*.crt) or a list of certificates (*.pem).
    ///     When performing TLS hanshake any of these certificates will be treated as a valid root for the certificate chain.
    /// </summary>
    /// <returns>
    ///     The path to the file that contains a certificate (*.crt) or a list of certificates (*.pem). The default value
    ///     is <see langword="null" />.
    /// </returns>
    public string? RootCertificate
    {
        get
        {
            var value = GetString(nameof(RootCertificate));
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }
        set => this[nameof(RootCertificate)] = value;
    }

    /// <summary>
    ///     Gets or sets the hash of the server's certificate in the hexadecimal format.
    ///     When performing TLS handshake the remote certificate with the specified hash will be treated as a valid certificate
    ///     despite any other certificate chain validation errors (e.g. invalid hostname).
    /// </summary>
    /// <returns>The hash of the server's certificate in the hexadecimal format. The default value is <see langword="null" />.</returns>
    public string? ServerCertificateHash
    {
        get
        {
            var value = GetString(nameof(ServerCertificateHash));
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }
        set => this[nameof(ServerCertificateHash)] = value;
    }

    public bool Pooling
    {
        get => GetBoolOrDefault(nameof(Pooling), true);
        set => this[nameof(Pooling)] = value;
    }

    public int MinSessions
    {
        get => GetInt32OrDefault(nameof(MinSessions), 1);
        set => this[nameof(MinSessions)] = value;
    }

    public int MaxSessions
    {
        get => GetInt32OrDefault(nameof(MaxSessions), 100);
        set => this[nameof(MaxSessions)] = value;
    }

    public bool UseSsl
    {
        get => GetBoolOrDefault(nameof(UseSsl), false);
        set => this[nameof(UseSsl)] = value;
    }

    public bool TrustSsl
    {
        get => GetBoolOrDefault(nameof(TrustSsl), false);
        set => this[nameof(TrustSsl)] = value;
    }

    private string? GetString(string key)
    {
        return TryGetValue(key, out var value) ? (string)value : null;
    }

    private string GetStringOrDefault(string key, string defaultValue)
    {
        if (!TryGetValue(key, out var value))
            return defaultValue;

        return (string)value ?? defaultValue;
    }

    private int GetInt32OrDefault(string key, int defaultValue)
    {
        if (!TryGetValue(key, out var value))
            return defaultValue;

        if (value is string strValue)
        {
            if (!int.TryParse(strValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                throw new InvalidOperationException($"The value of the property \"{key}\" must be an integer value.");

            return result;
        }

        return (int?)value ?? defaultValue;
    }

    private bool GetBoolOrDefault(string key, bool defaultValue)
    {
        if (!TryGetValue(key, out var value))
            return defaultValue;

        if (value is string strValue)
            switch (strValue.Trim().ToLowerInvariant())
            {
                case "on":
                case "true":
                case "1":
                    return true;

                case "off":
                case "false":
                case "0":
                    return false;

                default:
                    throw new InvalidOperationException(
                        $"The value of the property \"{key}\" is not a valid boolean value.");
            }

        return (bool?)value ?? defaultValue;
    }

    private TEnum GetEnumOrDefault<TEnum>(string key, TEnum defaultValue)
        where TEnum : struct
    {
        if (!TryGetValue(key, out var value) || value == null)
            return defaultValue;

        if (value is string strValue)
        {
            if (string.IsNullOrWhiteSpace(strValue))
                return defaultValue;

            // Enum.TryParse parses an integer value and casts it into enum without additional check.
            // Check that the value is not an integer before performing an actual enum parsing.
            if (int.TryParse(strValue.Trim(), out _) || !Enum.TryParse<TEnum>(strValue, true, out var result))
                throw new InvalidOperationException(
                    $"The value \"{strValue}\" is not a valid value for the property \"{key}\".");

            return result;
        }

        return (TEnum)value;
    }


    public YdbConnectionSettings Build()
    {
        return new YdbConnectionSettings(this);
    }

    public void PostProcessAndValidate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new ArgumentException("Host can't be null");
    }

    internal YDbConnectionStringBuilder Clone()
    {
        return new YDbConnectionStringBuilder(ConnectionString);
    }
}