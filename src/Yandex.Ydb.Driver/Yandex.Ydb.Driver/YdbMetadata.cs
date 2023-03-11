namespace Yandex.Ydb.Driver;

internal static class YdbMetadata
{
    public const string RpcDatabaseHeader = "x-ydb-database";
    public const string RpcAuthHeader = "x-ydb-auth-ticket";
    public const string RpcRequestTypeHeader = "x-ydb-request-type";
    public const string RpcTraceIdHeader = "x-ydb-trace-id";
    public const string RpcSdkInfoHeader = "x-ydb-sdk-build-info";
}

internal static class YdbDriverConstants
{
    public const string ExecuteDataQueryMethodOperationKey = "ExecuteDataQueryMethod";
}