using System.Diagnostics.CodeAnalysis;

namespace Yandex.Ydb.Driver.Helpers;

public static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowDriverException(Exception exception, string? msg = null)
    {
        throw new YdbDriverException(msg, exception);
    }

    [DoesNotReturn]
    public static void ThrowUnsupported(string msg)
    {
        ThrowDriverException(new NotSupportedException(msg));
    }

    [DoesNotReturn]
    public static void ThrowNullException(string param)
    {
        ThrowDriverException(new ArgumentNullException(param));
    }

    [DoesNotReturn]
    public static void ThrowObjectDisposedException(string? fullName)
    {
        throw new ObjectDisposedException(fullName);
    }

    [DoesNotReturn]
    public static void InvalidDataException(string msg)
    {
        ThrowDriverException(new InvalidDataException(msg));
    }
    
    [DoesNotReturn]
    public static void FileNotFound(string msg, string filename)
    {
        ThrowDriverException(new FileNotFoundException(msg, filename));
    }
}