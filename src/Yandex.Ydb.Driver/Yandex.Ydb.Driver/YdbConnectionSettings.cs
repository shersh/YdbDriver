using Yandex.Ydb.Driver.Helpers;

namespace Yandex.Ydb.Driver;

public sealed class YdbConnectionSettings
{
    public YdbConnectionSettings(YDbConnectionStringBuilder builder) : this(builder.Host, builder.Port,
        builder.User, builder.Password, builder.Database)
    {
    }

    public YdbConnectionSettings(string? host, ushort port, string? user, string? password, string database)
    {
        if (string.IsNullOrEmpty(host))
            ThrowHelper.ThrowNullException(nameof(host));

        Host = host;
        Port = port;
        User = user;
        Password = password;
        Database = database;
    }

    public string Host { get; init; }
    public ushort Port { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }
    public string Database { get; init; }

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