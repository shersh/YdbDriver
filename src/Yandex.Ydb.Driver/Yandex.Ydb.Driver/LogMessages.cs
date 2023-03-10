using Microsoft.Extensions.Logging;

namespace Yandex.Ydb.Driver;

// ReSharper disable InconsistentNaming
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
#pragma warning disable SYSLIB1006 // Multiple logging methods are using event id

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Opening connection to {host}:{port}/{database}")]
    internal static partial void OpeningConnection(ILogger logger, string host, int port, string database);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Execute command called with session `{session_id}`")]
    internal static partial void StartExecutingCommand(ILogger logger, string session_id);

    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Retry send command with session `{session_id}`, retried count: {count}")]
    internal static partial void RetryExecutingCommand(ILogger logger, string session_id, int count);


    [LoggerMessage(Level = LogLevel.Debug, Message = "Opening grpc channel to `{url}`")]
    internal static partial void OpenningGrpcChannel(ILogger logger, string url);
    
    
    [LoggerMessage(Level = LogLevel.Debug, Message = "Try create connection")]
    internal static partial void CreateDbConnection(ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create connection")]
    internal static partial void ExceptionToCreateDbConnection(ILogger logger, Exception ex);
}