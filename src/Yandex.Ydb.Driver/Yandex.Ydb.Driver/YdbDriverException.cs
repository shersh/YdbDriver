using System.Data.Common;

namespace Yandex.Ydb.Driver;

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