using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Yandex.Ydb.Driver.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LocalDatabaseFixture : IAsyncLifetime
{
    private readonly IMessageSink _sink;

    public LocalDatabaseFixture(IMessageSink sink)
    {
        _sink = sink;
    }
    
    public YdbDataSource Source { get; private set; }

    public void Dispose()
    {
        Source.Dispose();
    }

    public Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder().Build();
        var serviceProvider = new ServiceCollection()
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddXunit(_sink);
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            })
            .BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        var builder = new YdbDataSourceBuilder("Host=localhost;Port=2136;Pooling=true;MaxSessions=5");
        builder.UseLoggerFactory(factory);
        var source = builder.Build();
        Source = source;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Source.DisposeAsync();
    }
}