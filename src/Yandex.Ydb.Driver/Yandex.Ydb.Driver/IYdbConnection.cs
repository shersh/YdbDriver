using System.Data;

namespace Yandex.Ydb.Driver;

internal interface IYdbConnection : IDbConnection
{
    internal YdbConnector GetBoundConnector();

    internal string GetSessionId();
}