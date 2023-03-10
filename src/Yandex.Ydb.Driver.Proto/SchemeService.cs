// ReSharper disable once CheckNamespace

namespace Ydb.Scheme.V1;

public static partial class SchemeService
{
    public static Grpc.Core.Method<MakeDirectoryRequest, MakeDirectoryResponse> MakeDirectoryMethod => __Method_MakeDirectory;

    public static Grpc.Core.Method<RemoveDirectoryRequest, RemoveDirectoryResponse> RemoveDirectoryMethod => __Method_RemoveDirectory;

    public static Grpc.Core.Method<ListDirectoryRequest, ListDirectoryResponse> ListDirectoryMethod => __Method_ListDirectory;

    public static Grpc.Core.Method<DescribePathRequest, DescribePathResponse> DescribePathMethod => __Method_DescribePath;

    public static Grpc.Core.Method<ModifyPermissionsRequest, ModifyPermissionsResponse> ModifyPermissionsMethod => __Method_ModifyPermissions;
}