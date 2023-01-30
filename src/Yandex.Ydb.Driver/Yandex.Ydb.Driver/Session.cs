namespace Yandex.Ydb.Driver;

internal sealed class Session
{
    public Session(string id, string database)
    {
        Id = id;
        Database = database;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; }
    public string Database { get; }
    public DateTime CreatedAt { get; }
}