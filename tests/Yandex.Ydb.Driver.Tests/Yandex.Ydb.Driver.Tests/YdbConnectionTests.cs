using System.Data;

namespace Yandex.Ydb.Driver.Tests;

public class YdbConnectionTests : BaseTests
{
    [Fact]
    public async Task YdbConnection_BeginTransaction_Success()
    {
        await using var connection = await Source.OpenConnectionAsync();
        await using var transaction =
            await connection.BeginTransactionAsync(IsolationLevel.Serializable) as YdbTransaction;
        
        Assert.NotNull(transaction);
        Assert.NotNull(transaction.TransactionId);
        Assert.NotEmpty(transaction.TransactionId);
    }

    public YdbConnectionTests(LocalDatabaseFixture fixture) : base(fixture)
    {
    }
}