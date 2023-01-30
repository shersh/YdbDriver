namespace Yandex.Ydb.Driver;

public sealed class YdbDataSourceCommand : YdbCommand
{
    public YdbDataSourceCommand(YdbConnection connection) : base(connection)
    {
    }
}