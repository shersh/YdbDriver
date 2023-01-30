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
        if (op.IsFailed())
            throw new YdbDriverException(
                $"Operation `{op.Id}` failed. Status code: `{op.Status}`. Issues: `{string.Join("\n", op.Issues?.Select(x => x.ToString()) ?? Array.Empty<string>())}`");

        return !op.Result.TryUnpack(out T result)
            ? throw new YdbDriverException($"Failed to unpack result to type `{typeof(T)}`")
            : result;
    }
}