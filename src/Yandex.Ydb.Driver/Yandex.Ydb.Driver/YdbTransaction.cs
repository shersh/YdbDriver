using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb.Operations;
using Ydb.Table;
using Ydb.Table.V1;

namespace Yandex.Ydb.Driver;

public sealed class YdbTransaction : DbTransaction
{
    private readonly YdbConnection _connection;
    private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

    public YdbTransaction(YdbConnection connection)
    {
        _connection = connection;
    }

    protected override DbConnection? DbConnection => _connection;

    public override IsolationLevel IsolationLevel => _isolationLevel;
    public string TransactionId { get; private set; }

    public string Init(IsolationLevel isolationLevel)
    {
        Debug.Assert(_connection.Connector != null, "_connection.Connector != null");

        _isolationLevel = isolationLevel;
        var transactionSettings = new TransactionSettings();

        switch (isolationLevel)
        {
            case IsolationLevel.ReadUncommitted:
                transactionSettings.OnlineReadOnly = new OnlineModeSettings { AllowInconsistentReads = true };
                break;
            case IsolationLevel.ReadCommitted:
                transactionSettings.StaleReadOnly = new StaleModeSettings();
                break;
            case IsolationLevel.Serializable:
                transactionSettings.SerializableReadWrite = new SerializableModeSettings();
                break;
            case IsolationLevel.Unspecified:
            case IsolationLevel.Chaos:
            case IsolationLevel.RepeatableRead:
            default:
                throw new ArgumentOutOfRangeException(nameof(isolationLevel), isolationLevel, null);
        }

        var transaction = _connection.Connector.UnaryCall(TableService.BeginTransactionMethod,
            new BeginTransactionRequest
            {
                TxSettings = transactionSettings,
                SessionId = _connection.GetSessionId(),
                OperationParams = new OperationParams
                {
                    OperationMode = OperationParams.Types.OperationMode.Sync,
                    CancelAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(1)),
                    OperationTimeout = Duration.FromTimeSpan(TimeSpan.FromSeconds(1))
                }
            }, GetOptions());

        var result = transaction.Operation.GetResult<BeginTransactionResult>();
        TransactionId = result.TxMeta.Id;

        return TransactionId;
    }

    private CallOptions GetOptions()
    {
        return new CallOptions(new Metadata
        {
            { YdbMetadata.RpcDatabaseHeader, Connection?.Database ?? string.Empty }
        });
    }

    public override void Commit()
    {
        Debug.Assert(_connection.Connector != null, "_connection.Connector != null");

        var request = new CommitTransactionRequest
        {
            TxId = TransactionId,
            SessionId = _connection.GetSessionId()
        };

        var response = _connection.Connector.UnaryCall(TableService.CommitTransactionMethod, request, GetOptions());

        //TODO: Unpack only if in settings stats processing is set to true
        var result = response.Operation.GetResult<CommitTransactionResult>();
    }

    public override void Rollback()
    {
        Debug.Assert(_connection.Connector != null, "_connection.Connector != null");

        var request = new RollbackTransactionRequest
        {
            TxId = TransactionId,
            SessionId = _connection.GetSessionId()
        };

        var response = _connection.Connector.UnaryCall(TableService.RollbackTransactionMethod, request, GetOptions());
        if (response.Operation.IsFailed())
            throw new YdbDriverException(
                $"Failed to rollback transaction `{TransactionId}`. Status: `{response.Operation.Status}`");
    }
}