using Microsoft.Extensions.Logging;

namespace Yandex.Ydb.Driver;

// ReSharper disable InconsistentNaming
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
#pragma warning disable SYSLIB1006 // Multiple logging methods are using event id

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Opening connection to {host}:{port}/{database}")]
    internal static partial void OpeningConnection(ILogger logger, string host, int port, string database);
}