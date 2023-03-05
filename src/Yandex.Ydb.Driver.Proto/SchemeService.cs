// ReSharper disable once CheckNamespace

using System.CodeDom.Compiler;
using Grpc.Core;

namespace Ydb.Scheme.V1;

public static partial class SchemeService
{
    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<MakeDirectoryRequest, MakeDirectoryResponse> MakeDirectoryMethod { get; } =
        new Method<MakeDirectoryRequest, MakeDirectoryResponse>(
            MethodType.Unary,
            __ServiceName,
            "MakeDirectory",
            __Marshaller_Ydb_Scheme_MakeDirectoryRequest,
            __Marshaller_Ydb_Scheme_MakeDirectoryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<RemoveDirectoryRequest, RemoveDirectoryResponse> RemoveDirectoryMethod { get; } =
        new Method<RemoveDirectoryRequest, RemoveDirectoryResponse>(
            MethodType.Unary,
            __ServiceName,
            "RemoveDirectory",
            __Marshaller_Ydb_Scheme_RemoveDirectoryRequest,
            __Marshaller_Ydb_Scheme_RemoveDirectoryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ListDirectoryRequest, ListDirectoryResponse> ListDirectoryMethod { get; } =
        new Method<ListDirectoryRequest, ListDirectoryResponse>(
            MethodType.Unary,
            __ServiceName,
            "ListDirectory",
            __Marshaller_Ydb_Scheme_ListDirectoryRequest,
            __Marshaller_Ydb_Scheme_ListDirectoryResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<DescribePathRequest, DescribePathResponse> DescribePathMethod { get; } =
        new Method<DescribePathRequest, DescribePathResponse>(
            MethodType.Unary,
            __ServiceName,
            "DescribePath",
            __Marshaller_Ydb_Scheme_DescribePathRequest,
            __Marshaller_Ydb_Scheme_DescribePathResponse);

    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<ModifyPermissionsRequest, ModifyPermissionsResponse> ModifyPermissionsMethod { get; } =
        new Method<ModifyPermissionsRequest, ModifyPermissionsResponse>(
            MethodType.Unary,
            __ServiceName,
            "ModifyPermissions",
            __Marshaller_Ydb_Scheme_ModifyPermissionsRequest,
            __Marshaller_Ydb_Scheme_ModifyPermissionsResponse);
}