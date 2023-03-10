using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Yandex.Ydb.Driver.Tests;

public sealed class XunitLoggerProvider : ILoggerProvider
{
    private readonly IDisposable _configurationOnChangeToken;
    private XunitLoggerConfiguration _currentConfiguration;
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = new();
    private readonly IExternalScopeProvider _externalScopeProvider = new LoggerExternalScopeProvider();
    private readonly IMessageSink _testOutputHelper;

    public XunitLoggerProvider(IOptionsMonitor<XunitLoggerConfiguration> optionsMonitor,
        IMessageSink testOutputHelper)
    {
        _currentConfiguration = optionsMonitor.CurrentValue;
        _configurationOnChangeToken =
            optionsMonitor.OnChange(updatedConfiguration => _currentConfiguration = updatedConfiguration);
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        var logger = _loggers.GetOrAdd(categoryName,
            name => new XunitLogger(name, GetCurrentConfiguration, _externalScopeProvider, _testOutputHelper));
        return logger;
    }

    public void Dispose()
    {
        _loggers.Clear();
        _configurationOnChangeToken.Dispose();
    }

    private XunitLoggerConfiguration GetCurrentConfiguration() => _currentConfiguration;
}