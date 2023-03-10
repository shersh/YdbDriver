using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Xunit.Abstractions;

namespace Yandex.Ydb.Driver.Tests;

public static class XunitLoggingBuilderExtensions
{
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, IMessageSink testOutputHelper)
    {
        builder.AddConfiguration();

        builder.Services.TryAddSingleton(testOutputHelper);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, XunitLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <XunitLoggerConfiguration, XunitLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, IMessageSink testOutputHelper,
        Action<XunitLoggerConfiguration> configure)
    {
        builder.AddXunit(testOutputHelper);
        builder.Services.Configure(configure);

        return builder;
    }
}