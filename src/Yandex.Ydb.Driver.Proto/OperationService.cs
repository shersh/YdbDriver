using Ydb.Operations;

// ReSharper disable once CheckNamespace
namespace Ydb.Operation.V1;

public static partial class OperationService
{
    public static Grpc.Core.Method<GetOperationRequest, GetOperationResponse> GetOperationMethod =>
        __Method_GetOperation;

    public static Grpc.Core.Method<CancelOperationRequest, CancelOperationResponse> CancelOperationMethod =>
        __Method_CancelOperation;

    public static Grpc.Core.Method<ForgetOperationRequest, ForgetOperationResponse> ForgetOperationMethod =>
        __Method_ForgetOperation;

    public static Grpc.Core.Method<ListOperationsRequest, ListOperationsResponse> ListOperationsMethod =>
        __Method_ListOperations;
}