using Microsoft.Extensions.Logging;
using Ydb;

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

    [LoggerMessage(Level = LogLevel.Debug, Message = "Try create connection")]
    internal static partial void CreateDbConnection(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create connection")]
    internal static partial void ExceptionToCreateDbConnection(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning,
        Message =
            "Retry operation `{operation_id}` with `{attempt}` attempt after waiting `{duration}`. Last operation code: {status_code}")]
    internal static partial void WaitAndRetryOperation(ILogger logger, string operation_id, int attempt,
        TimeSpan duration, StatusIds.Types.StatusCode status_code);

    [LoggerMessage(Level = LogLevel.Warning,
        Message =
            "Operation `{operation_id}` is failed with status `{status_code}`")]
    internal static partial void ExecuteDataQueryFailed(ILogger logger, string operation_id,
        StatusIds.Types.StatusCode status_code);

    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Executing data query for operation with correlation id `{correlation_id}`. Query: `{query}`")]
    internal static partial void ExecuteDataQueryCalled(ILogger logger, Guid correlation_id, string query);
}