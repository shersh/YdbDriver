using Ydb.Table;

namespace Yandex.Ydb.Driver;

public static class Common
{
    public static readonly TransactionControl DefaultTxControl = new()
    {
        BeginTx = new TransactionSettings
        {
            SerializableReadWrite = new SerializableModeSettings()
        },
        CommitTx = true
    };
}