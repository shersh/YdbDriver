namespace Yandex.Ydb.Driver.Tests;

public abstract class BaseTests : IClassFixture<LocalDatabaseFixture>
{
    protected LocalDatabaseFixture Fixture { get; }

    protected YdbDataSource Source => Fixture.Source;

    protected BaseTests(LocalDatabaseFixture fixture)
    {
        Fixture = fixture;
    }
}