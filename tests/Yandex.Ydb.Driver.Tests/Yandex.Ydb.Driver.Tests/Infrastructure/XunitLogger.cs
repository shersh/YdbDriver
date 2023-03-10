using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Yandex.Ydb.Driver.Tests;

public class XunitLogger : ILogger
{
    private readonly string _loggerName;
    private readonly Func<XunitLoggerConfiguration> _getCurrentConfig;
    private readonly IExternalScopeProvider _externalScopeProvider;
    private readonly IMessageSink _testOutputHelper;

    public XunitLogger(string loggerName, Func<XunitLoggerConfiguration> getCurrentConfig,
        IExternalScopeProvider externalScopeProvider, IMessageSink testOutputHelper)
    {
        _loggerName = loggerName;
        _getCurrentConfig = getCurrentConfig;
        _externalScopeProvider = externalScopeProvider;
        _testOutputHelper = testOutputHelper;
    }

    public IDisposable BeginScope<TState>(TState state) => _externalScopeProvider.Push(state);

    public bool IsEnabled(LogLevel logLevel) => LogLevel.None != logLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        _testOutputHelper.OnMessage(new DiagnosticMessage(message));
    }
}