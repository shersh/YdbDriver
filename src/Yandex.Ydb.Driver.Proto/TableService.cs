// ReSharper disable once CheckNamespace

using System.CodeDom.Compiler;
using Grpc.Core;

namespace Ydb.Table.V1;

public static partial class TableService
{
    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CreateSessionRequest, CreateSessionResponse>
        CreateSessionMethod { get; } = new Method<CreateSessionRequest, CreateSessionResponse>(
        MethodType.Unary,
        __ServiceName,
        "CreateSession",
        __Marshaller_Ydb_Table_CreateSessionRequest,
        __Marshaller_Ydb_Table_CreateSessionResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<DeleteSessionRequest, DeleteSessionResponse> DeleteSessionMethod { get; } =
        new Method<DeleteSessionRequest, DeleteSessionResponse>(
            MethodType.Unary,
            __ServiceName,
            "DeleteSession",
            __Marshaller_Ydb_Table_DeleteSessionRequest,
            __Marshaller_Ydb_Table_DeleteSessionResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<KeepAliveRequest, KeepAliveResponse> KeepAliveMethod { get; } =
        new Method<KeepAliveRequest, KeepAliveResponse>(
            MethodType.Unary,
            __ServiceName,
            "KeepAlive",
            __Marshaller_Ydb_Table_KeepAliveRequest,
            __Marshaller_Ydb_Table_KeepAliveResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CreateTableRequest, CreateTableResponse> CreateTableMethod { get; } =
        new Method<CreateTableRequest, CreateTableResponse>(
            MethodType.Unary,
            __ServiceName,
            "CreateTable",
            __Marshaller_Ydb_Table_CreateTableRequest,
            __Marshaller_Ydb_Table_CreateTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<DropTableRequest, DropTableResponse> DropTableMethod { get; } =
        new Method<DropTableRequest, DropTableResponse>(
            MethodType.Unary,
            __ServiceName,
            "DropTable",
            __Marshaller_Ydb_Table_DropTableRequest,
            __Marshaller_Ydb_Table_DropTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<AlterTableRequest, AlterTableResponse> AlterTableMethod { get; } =
        new Method<AlterTableRequest, AlterTableResponse>(
            MethodType.Unary,
            __ServiceName,
            "AlterTable",
            __Marshaller_Ydb_Table_AlterTableRequest,
            __Marshaller_Ydb_Table_AlterTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CopyTableRequest, CopyTableResponse> CopyTableMethod { get; } =
        new Method<CopyTableRequest, CopyTableResponse>(
            MethodType.Unary,
            __ServiceName,
            "CopyTable",
            __Marshaller_Ydb_Table_CopyTableRequest,
            __Marshaller_Ydb_Table_CopyTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CopyTablesRequest, CopyTablesResponse> CopyTablesMethod { get; } =
        new Method<CopyTablesRequest, CopyTablesResponse>(
            MethodType.Unary,
            __ServiceName,
            "CopyTables",
            __Marshaller_Ydb_Table_CopyTablesRequest,
            __Marshaller_Ydb_Table_CopyTablesResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<RenameTablesRequest, RenameTablesResponse> RenameTablesMethod { get; } =
        new Method<RenameTablesRequest, RenameTablesResponse>(
            MethodType.Unary,
            __ServiceName,
            "RenameTables",
            __Marshaller_Ydb_Table_RenameTablesRequest,
            __Marshaller_Ydb_Table_RenameTablesResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<DescribeTableRequest, DescribeTableResponse> DescribeTableMethod { get; } =
        new Method<DescribeTableRequest, DescribeTableResponse>(
            MethodType.Unary,
            __ServiceName,
            "DescribeTable",
            __Marshaller_Ydb_Table_DescribeTableRequest,
            __Marshaller_Ydb_Table_DescribeTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ExplainDataQueryRequest, ExplainDataQueryResponse> ExplainDataQueryMethod { get; } =
        new Method<ExplainDataQueryRequest, ExplainDataQueryResponse>(
            MethodType.Unary,
            __ServiceName,
            "ExplainDataQuery",
            __Marshaller_Ydb_Table_ExplainDataQueryRequest,
            __Marshaller_Ydb_Table_ExplainDataQueryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<PrepareDataQueryRequest, PrepareDataQueryResponse> PrepareDataQueryMethod { get; } =
        new Method<PrepareDataQueryRequest, PrepareDataQueryResponse>(
            MethodType.Unary,
            __ServiceName,
            "PrepareDataQuery",
            __Marshaller_Ydb_Table_PrepareDataQueryRequest,
            __Marshaller_Ydb_Table_PrepareDataQueryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ExecuteDataQueryRequest, ExecuteDataQueryResponse> ExecuteDataQueryMethod { get; } =
        new Method<ExecuteDataQueryRequest, ExecuteDataQueryResponse>(
            MethodType.Unary,
            __ServiceName,
            "ExecuteDataQuery",
            __Marshaller_Ydb_Table_ExecuteDataQueryRequest,
            __Marshaller_Ydb_Table_ExecuteDataQueryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ExecuteSchemeQueryRequest, ExecuteSchemeQueryResponse> ExecuteSchemeQueryMethod { get; } =
        new Method<ExecuteSchemeQueryRequest, ExecuteSchemeQueryResponse>(
            MethodType.Unary,
            __ServiceName,
            "ExecuteSchemeQuery",
            __Marshaller_Ydb_Table_ExecuteSchemeQueryRequest,
            __Marshaller_Ydb_Table_ExecuteSchemeQueryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<BeginTransactionRequest, BeginTransactionResponse> BeginTransactionMethod { get; } =
        new Method<BeginTransactionRequest, BeginTransactionResponse>(
            MethodType.Unary,
            __ServiceName,
            "BeginTransaction",
            __Marshaller_Ydb_Table_BeginTransactionRequest,
            __Marshaller_Ydb_Table_BeginTransactionResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CommitTransactionRequest, CommitTransactionResponse> CommitTransactionMethod { get; } =
        new Method<CommitTransactionRequest, CommitTransactionResponse>(
            MethodType.Unary,
            __ServiceName,
            "CommitTransaction",
            __Marshaller_Ydb_Table_CommitTransactionRequest,
            __Marshaller_Ydb_Table_CommitTransactionResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<RollbackTransactionRequest, RollbackTransactionResponse> RollbackTransactionMethod { get; } =
        new Method<RollbackTransactionRequest, RollbackTransactionResponse>(
            MethodType.Unary,
            __ServiceName,
            "RollbackTransaction",
            __Marshaller_Ydb_Table_RollbackTransactionRequest,
            __Marshaller_Ydb_Table_RollbackTransactionResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<DescribeTableOptionsRequest, DescribeTableOptionsResponse>
        DescribeTableOptionsMethod { get; } = new Method<DescribeTableOptionsRequest, DescribeTableOptionsResponse>(
        MethodType.Unary,
        __ServiceName,
        "DescribeTableOptions",
        __Marshaller_Ydb_Table_DescribeTableOptionsRequest,
        __Marshaller_Ydb_Table_DescribeTableOptionsResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ReadTableRequest, ReadTableResponse> StreamReadTableMethod { get; } =
        new Method<ReadTableRequest, ReadTableResponse>(
            MethodType.ServerStreaming,
            __ServiceName,
            "StreamReadTable",
            __Marshaller_Ydb_Table_ReadTableRequest,
            __Marshaller_Ydb_Table_ReadTableResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<BulkUpsertRequest, BulkUpsertResponse> BulkUpsertMethod { get; } =
        new Method<BulkUpsertRequest, BulkUpsertResponse>(
            MethodType.Unary,
            __ServiceName,
            "BulkUpsert",
            __Marshaller_Ydb_Table_BulkUpsertRequest,
            __Marshaller_Ydb_Table_BulkUpsertResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ExecuteScanQueryRequest, ExecuteScanQueryPartialResponse>
        StreamExecuteScanQueryMethod { get; } = new Method<ExecuteScanQueryRequest, ExecuteScanQueryPartialResponse>(
        MethodType.ServerStreaming,
        __ServiceName,
        "StreamExecuteScanQuery",
        __Marshaller_Ydb_Table_ExecuteScanQueryRequest,
        __Marshaller_Ydb_Table_ExecuteScanQueryPartialResponse);
}