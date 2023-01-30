// ReSharper disable once CheckNamespace
namespace Ydb.Table.V1;

public static partial class TableService
{
    public static Grpc.Core.Method<CreateSessionRequest, CreateSessionResponse>
        CreateSessionMethod => __Method_CreateSession;

    public static Grpc.Core.Method<DeleteSessionRequest, DeleteSessionResponse> DeleteSessionMethod =>
        __Method_DeleteSession;

    public static Grpc.Core.Method<KeepAliveRequest, KeepAliveResponse> KeepAliveMethod => __Method_KeepAlive;

    public static Grpc.Core.Method<CreateTableRequest, CreateTableResponse> CreateTableMethod => __Method_CreateTable;

    public static Grpc.Core.Method<DropTableRequest, DropTableResponse> DropTableMethod => __Method_DropTable;

    public static Grpc.Core.Method<AlterTableRequest, AlterTableResponse> AlterTableMethod => __Method_AlterTable;

    public static Grpc.Core.Method<CopyTableRequest, CopyTableResponse> CopyTableMethod => __Method_CopyTable;

    public static Grpc.Core.Method<CopyTablesRequest, CopyTablesResponse> CopyTablesMethod => __Method_CopyTables;

    public static Grpc.Core.Method<RenameTablesRequest, RenameTablesResponse> RenameTablesMethod =>
        __Method_RenameTables;

    public static Grpc.Core.Method<DescribeTableRequest, DescribeTableResponse> DescribeTableMethod =>
        __Method_DescribeTable;

    public static Grpc.Core.Method<ExplainDataQueryRequest, ExplainDataQueryResponse> ExplainDataQueryMethod =>
        __Method_ExplainDataQuery;

    public static Grpc.Core.Method<PrepareDataQueryRequest, PrepareDataQueryResponse> PrepareDataQueryMethod =>
        __Method_PrepareDataQuery;

    public static Grpc.Core.Method<ExecuteDataQueryRequest, ExecuteDataQueryResponse> ExecuteDataQueryMethod =>
        __Method_ExecuteDataQuery;

    public static Grpc.Core.Method<ExecuteSchemeQueryRequest, ExecuteSchemeQueryResponse> ExecuteSchemeQueryMethod =>
        __Method_ExecuteSchemeQuery;

    public static Grpc.Core.Method<BeginTransactionRequest, BeginTransactionResponse> BeginTransactionMethod =>
        __Method_BeginTransaction;

    public static Grpc.Core.Method<CommitTransactionRequest, CommitTransactionResponse> CommitTransactionMethod =>
        __Method_CommitTransaction;

    public static Grpc.Core.Method<RollbackTransactionRequest, RollbackTransactionResponse> RollbackTransactionMethod =>
        __Method_RollbackTransaction;

    public static Grpc.Core.Method<DescribeTableOptionsRequest, DescribeTableOptionsResponse>
        DescribeTableOptionsMethod => __Method_DescribeTableOptions;

    public static Grpc.Core.Method<ReadTableRequest, ReadTableResponse> StreamReadTableMethod =>
        __Method_StreamReadTable;

    public static Grpc.Core.Method<BulkUpsertRequest, BulkUpsertResponse> BulkUpsertMethod => __Method_BulkUpsert;

    public static Grpc.Core.Method<ExecuteScanQueryRequest, ExecuteScanQueryPartialResponse>
        StreamExecuteScanQueryMethod => __Method_StreamExecuteScanQuery;
}