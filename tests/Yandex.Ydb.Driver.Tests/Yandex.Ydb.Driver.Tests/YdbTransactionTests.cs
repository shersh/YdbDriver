using System.Data;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Yandex.Ydb.Driver.Tests;

public class YdbTransactionTests : IClassFixture<LocalDatabaseFixture>
{
    private readonly LocalDatabaseFixture _fixture;

    public YdbTransactionTests(LocalDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void YdbTransaction_Commit_Success()
    {
        using var connection = _fixture.Source.OpenConnection();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        transaction.Commit();
    }

    [Fact]
    public void YdbTransaction_Rollback_Success()
    {
        using var connection = _fixture.Source.OpenConnection();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        transaction.Rollback();
    }
}