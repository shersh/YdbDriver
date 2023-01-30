using System.Data;

namespace Yandex.Ydb.Driver.Tests;

public class YdbTransactionTests
{
    [Fact]
    public void YdbTransaction_Commit_Success()
    {
        using var connection = TestHelper.GetDefaultConnectionAndOpen();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        transaction.Commit();
    }

    [Fact]
    public void YdbTransaction_Rollback_Success()
    {
        using var connection = TestHelper.GetDefaultConnectionAndOpen();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        transaction.Rollback();
    }
}