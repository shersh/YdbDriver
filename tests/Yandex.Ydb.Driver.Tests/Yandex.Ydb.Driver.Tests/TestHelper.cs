namespace Yandex.Ydb.Driver.Tests;

public static class TestHelper
{
    public static YdbConnection GetDefaultConnectionAndOpen()
    {
        var source = YdbDataSource.Create("Host=localhost;Port=2136");
        var connection = source.CreateConnection();

        connection.Open();

        return connection;
    }
}