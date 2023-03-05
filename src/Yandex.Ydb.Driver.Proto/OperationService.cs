using System.CodeDom.Compiler;
using Grpc.Core;
using Ydb.Operations;

// ReSharper disable once CheckNamespace
namespace Ydb.Operation.V1;

public static partial class OperationService
{
    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<GetOperationRequest, GetOperationResponse> GetOperationMethod { get; } =
        new Method<GetOperationRequest, GetOperationResponse>(
            MethodType.Unary,
            __ServiceName,
            "GetOperation",
            __Marshaller_Ydb_Operations_GetOperationRequest,
            __Marshaller_Ydb_Operations_GetOperationResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CancelOperationRequest, CancelOperationResponse> CancelOperationMethod { get; } =
        new Method<CancelOperationRequest, CancelOperationResponse>(
            MethodType.Unary,
            __ServiceName,
            "CancelOperation",
            __Marshaller_Ydb_Operations_CancelOperationRequest,
            __Marshaller_Ydb_Operations_CancelOperationResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ForgetOperationRequest, ForgetOperationResponse> ForgetOperationMethod { get; } =
        new Method<ForgetOperationRequest, ForgetOperationResponse>(
            MethodType.Unary,
            __ServiceName,
            "ForgetOperation",
            __Marshaller_Ydb_Operations_ForgetOperationRequest,
            __Marshaller_Ydb_Operations_ForgetOperationResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ListOperationsRequest, ListOperationsResponse> ListOperationsMethod { get; } =
        new Method<ListOperationsRequest, ListOperationsResponse>(
            MethodType.Unary,
            __ServiceName,
            "ListOperations",
            __Marshaller_Ydb_Operations_ListOperationsRequest,
            __Marshaller_Ydb_Operations_ListOperationsResponse);
}