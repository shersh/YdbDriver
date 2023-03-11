using Google.Protobuf;
using Ydb;
using Ydb.Operations;

namespace Yandex.Ydb.Driver.Internal.TypeHandling;

public static class OperationExtensions
{
    public static bool IsFailed(this Operation op)
    {
        return !IsSuccess(op);
    }

    public static bool IsSuccess(this Operation op)
    {
        return op.Status == StatusIds.Types.StatusCode.Success;
    }

    public static T GetResult<T>(this Operation op) where T : IMessage, new()
    {
        return !op.Result.TryUnpack(out T result)
            ? throw new YdbUnpackException(op, typeof(T))
            : result;
    }
}