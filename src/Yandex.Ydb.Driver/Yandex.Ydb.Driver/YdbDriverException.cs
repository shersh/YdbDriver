using System.Data.Common;
using Ydb.Operations;
using Ydb.Table;

namespace Yandex.Ydb.Driver;

public class YdbOperationFailedException : YdbDriverException
{
    public Operation Operation { get; }

    public YdbOperationFailedException(Operation operation) : base(
        $"Operation failed with status `{operation.Status}`")
    {
        Operation = operation;
    }
}

public class YdbUnpackException : YdbDriverException
{
    public Operation Operation { get; }
    public Type UnpackType { get; }

    public YdbUnpackException(Operation operation, Type unpackType) : base(
        $"Failed to unpack operation result to `{unpackType.FullName}`")
    {
        Operation = operation;
        UnpackType = unpackType;
    }
}

public class YdbDriverException : DbException
{
    public YdbDriverException()
    {
    }

    public YdbDriverException(string msg) : base(msg)
    {
    }

    public YdbDriverException(string? msg, Exception exception) : base(msg, exception)
    {
    }

    public YdbDriverException(string? msg, int error) : base(msg, error)
    {
    }
}