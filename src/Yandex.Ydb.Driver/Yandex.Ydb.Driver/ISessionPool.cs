namespace Yandex.Ydb.Driver;

internal interface ISessionPool : IDisposable
{
    internal void Return(string sessionId);
    internal ValueTask<string> GetSession(string database);

    internal Task Initialize(string database);
}