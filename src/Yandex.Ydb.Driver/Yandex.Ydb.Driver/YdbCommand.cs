using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;
using Ydb.Operations;
using Ydb.Table;
using Ydb.Table.V1;

namespace Yandex.Ydb.Driver;

/// <inheritdoc />
public class YdbCommand : DbCommand
{
    private readonly DbParameterCollection _parameters = new YdbParameterCollection();
    private string _queryId;
    private YdbConnection _ydbConnection;

    /// <inheritdoc />
    public YdbCommand(YdbConnection ydbConnection)
    {
        _ydbConnection = ydbConnection;
    }

    public TransactionControl? TxControl { get; set; }

    /// <summary>
    ///     Gets flag which indicated that command already prepared
    /// </summary>
    public bool IsPrepared { get; private set; }

    /// <summary>Gets or sets the text command to run against the data source.</summary>
    /// <returns>The text command to execute. The default value is an empty string ("").</returns>
    public override string CommandText { get; set; }

    /// <summary>
    ///     Gets or sets the wait time (in seconds) before terminating the attempt to execute the command and generating
    ///     an error.
    /// </summary>
    /// <returns>The time in seconds to wait for the command to execute.</returns>
    public override int CommandTimeout { get; set; }

    /// <inheritdoc />
    public override CommandType CommandType
    {
        get => CommandType.Text;
        set => throw new YdbDriverException(null,
            new NotSupportedException("Changing CommandType is not supported by this driver"));
    }

    /// <inheritdoc />
    public override UpdateRowSource UpdatedRowSource
    {
        get => UpdateRowSource.Both;
        set =>
            throw new YdbDriverException(null,
                new NotSupportedException("Changing UpdatedRowSource is not supported by this driver"));
    }

    /// <inheritdoc />
    protected override DbConnection? DbConnection
    {
        get => _ydbConnection;
        set => _ydbConnection = (value as YdbConnection)!;
    }

    protected ILogger Logger => _ydbConnection.GetCommandLogger();

    /// <inheritdoc />
    protected override DbParameterCollection DbParameterCollection => _parameters;

    /// <inheritdoc />
    protected override DbTransaction? DbTransaction { get; set; }

    /// <inheritdoc />
    public override bool DesignTimeVisible { get; set; }

    /// <summary>Attempts to cancel the execution of a <see cref="T:System.Data.Common.DbCommand" />.</summary>
    public override void Cancel()
    {
        throw new NotSupportedException("Cancelling command is not supported");
    }

    /// <inheritdoc />
    public override int ExecuteNonQuery()
    {
        var result = ExecuteQueryAsync().GetAwaiter().GetResult();
        return result.ResultSets.Count;
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        var result = await ExecuteQueryAsync();
        return result.ResultSets.Count;
    }

    private async Task<ExecuteQueryResult> ExecuteQueryAsync()
    {
        var sessionId = _ydbConnection.GetSessionId();
        LogMessages.StartExecutingCommand(Logger, sessionId);

        Debug.Assert(_ydbConnection.Connector != null);

        var transactionControl = TxControl ?? Common.DefaultTxControl.Clone();

        if (DbTransaction is YdbTransaction ydbTransaction)
        {
            transactionControl = new TransactionControl
            {
                TxId = ydbTransaction.TransactionId
            };

            if (ydbTransaction.IsolationLevel != IsolationLevel.Serializable)
                transactionControl.CommitTx = true;
        }

        var request = new ExecuteDataQueryRequest
        {
            SessionId = sessionId,
            Query = new Query
            {
                YqlText = CommandText
            },
            QueryCachePolicy = new QueryCachePolicy
            {
                KeepInCache = _parameters.Count > 0
            },
            TxControl = transactionControl,
            OperationParams = new OperationParams
            {
                OperationMode = OperationParams.Types.OperationMode.Sync,
                ReportCostInfo = FeatureFlag.Types.Status.Disabled
            }
        };

        foreach (YdbParameter parameter in _parameters)
        {
            parameter.ResolveHandler(_ydbConnection.TypeMapper);
            request.Parameters.Add(parameter.ParameterName, parameter.ToProto());
        }

        ExecuteDataQueryResponse? response = null;

        for (var i = 0; i < 10; i++)
        {
            response = await _ydbConnection.Connector.UnaryCallAsync(TableService.ExecuteDataQueryMethod, request,
                GetOptions());
            if (!response.Operation.Ready)
                throw new YdbDriverException($"Operation `{response.Operation.Id}` is not ready");

            if (response.Operation.Status == StatusIds.Types.StatusCode.Success) break;

            LogMessages.RetryExecutingCommand(Logger, sessionId, i + 1);
            await Task.Delay(i); //RETRY
            //TODO: add applying Backoff policy
        }

        var result = response.Operation.GetResult<ExecuteQueryResult>();
        return result;
    }

    private CallOptions GetOptions()
    {
        return new CallOptions(new Metadata
        {
            { YdbMetadata.RpcDatabaseHeader, Connection?.Database ?? string.Empty }
        });
    }

    /// <inheritdoc />
    public override object? ExecuteScalar()
    {
        var reader = ExecuteDbDataReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        return !reader.Read() ? null : reader.GetValue(0);
    }

    /// <inheritdoc />
    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteQueryAsync();
        var dbDataReader = new YdbDbDataReader(result, _ydbConnection.TypeMapper);
        await dbDataReader.NextResultAsync(cancellationToken);
        return dbDataReader;
    }

    /// <inheritdoc />
    public override void Prepare()
    {
        if (IsPrepared)
            return;

        Debug.Assert(_ydbConnection.Connector != null, "_ydbConnection.Connector != null");

        var response = _ydbConnection.Connector.UnaryCall(TableService.PrepareDataQueryMethod,
            new PrepareDataQueryRequest
            {
                SessionId = _ydbConnection.GetSessionId(), YqlText = CommandText, OperationParams = GetOperationParams()
            }, GetOptions());

        var result = response.Operation.GetResult<PrepareQueryResult>();
        _queryId = result.QueryId;

        IsPrepared = true;
    }

    private OperationParams GetOperationParams()
    {
        return new OperationParams
        {
            CancelAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(30)),
            OperationTimeout = Duration.FromTimeSpan(TimeSpan.FromSeconds(30)),
            OperationMode = OperationParams.Types.OperationMode.Sync,
            ReportCostInfo = FeatureFlag.Types.Status.Disabled
        };
    }

    /// <inheritdoc />
    protected override DbParameter CreateDbParameter()
    {
        return new YdbParameter();
    }

    /// <inheritdoc />
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var result = ExecuteQueryAsync().GetAwaiter().GetResult();
        return new YdbDbDataReader(result, _ydbConnection.TypeMapper);
    }

    public void AddParameter<T>(string name, T value)
    {
        _parameters.Add(new YdbParameter<T>(name, value));
    }
}