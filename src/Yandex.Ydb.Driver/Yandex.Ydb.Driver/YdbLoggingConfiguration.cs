using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Yandex.Ydb.Driver;

public class YdbLoggingConfiguration
{
    internal static readonly YdbLoggingConfiguration NullConfiguration
        = new(NullLoggerFactory.Instance);

    internal static ILoggerFactory GlobalLoggerFactory = NullLoggerFactory.Instance;

    public YdbLoggingConfiguration(ILoggerFactory loggerFactory)
    {
        ConnectionLogger = loggerFactory.CreateLogger("Ydb.Connection");
        CommandLogger = loggerFactory.CreateLogger("Ydb.Command");
        TransactionLogger = loggerFactory.CreateLogger("Ydb.Transaction");
        ExceptionLogger = loggerFactory.CreateLogger("Ydb.Exception");
    }

    internal ILogger ConnectionLogger { get; }
    internal ILogger CommandLogger { get; }
    internal ILogger TransactionLogger { get; }
    internal ILogger ExceptionLogger { get; }

    public static void InitializeLogging(ILoggerFactory loggerFactory)
    {
        GlobalLoggerFactory = loggerFactory;
    }
}