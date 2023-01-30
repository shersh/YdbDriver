using System.Data;

namespace Yandex.Ydb.Driver.Tests;

public class YdbConnectionTests
{
    [Fact]
    public void YdbConnection_BeginTransaction_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var transaction = connection.BeginTransaction(IsolationLevel.Serializable) as YdbTransaction;
        Assert.NotNull(transaction);
        Assert.NotNull(transaction.TransactionId);
        Assert.NotEmpty(transaction.TransactionId);
    }
}