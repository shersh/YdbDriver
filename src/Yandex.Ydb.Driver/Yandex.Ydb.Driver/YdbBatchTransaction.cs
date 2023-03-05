using System.Data;
using System.Data.Common;

namespace Yandex.Ydb.Driver;

public sealed class YdbBatchTransaction : DbTransaction
{
    private readonly List<YdbCommand> _commands = new();
    private readonly YdbConnection _connection;

    public YdbBatchTransaction(YdbConnection connection)
    {
        _connection = connection;
    }

    protected override DbConnection? DbConnection { get; }
    public override IsolationLevel IsolationLevel { get; }

    public void Append(YdbCommand command)
    {
        _commands.Add(command);
    }

    public override void Commit()
    {
        foreach (var cmd in _commands) cmd.ExecuteNonQuery();

        _commands.Clear();
    }

    public override void Rollback()
    {
        _commands.Clear();
    }
}