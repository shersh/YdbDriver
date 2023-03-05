using Yandex.Ydb.Driver.Helpers;

namespace Yandex.Ydb.Driver;

public sealed class YdbConnectionSettings
{
    public YdbConnectionSettings(YDbConnectionStringBuilder builder) : this(builder.Host, builder.Port,
        builder.User, builder.Password, builder.Database, builder.UseSsl)
    {
    }

    public YdbConnectionSettings(string? host, ushort port, string? user, string? password, string database,
        bool useSsl)
    {
        if (string.IsNullOrEmpty(host))
            ThrowHelper.ThrowNullException(nameof(host));

        Host = host;
        Port = port;
        User = user;
        Password = password;
        Database = database;
        UseSsl = useSsl;
    }

    public string Host { get; init; }
    public ushort Port { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }
    public string Database { get; init; }
    public bool UseSsl { get; init; }

    public void Deconstruct(out string host, out ushort port, out string? user, out string? password,
        out string database)
    {
        host = Host;
        port = Port;
        user = User;
        password = Password;
        database = Database;
    }
}