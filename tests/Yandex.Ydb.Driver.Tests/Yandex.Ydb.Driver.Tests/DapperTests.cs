using Dapper;

namespace Yandex.Ydb.Driver.Tests;

public class DapperTests : BaseTests
{
    [Fact]
    public async Task QueryDynamic_Success()
    {
        await using var connection = await Source.OpenConnectionAsync();
        var queryAsync = await connection.QueryAsync(@"SELECT 1;");

        Assert.NotEmpty(queryAsync);
    }

    [Fact]
    public async Task QueryCast_Success()
    {
        await using var connection = await Source.OpenConnectionAsync();
        var queryAsync = await connection.QueryAsync<TestClass>(@"SELECT 123 as id;");

        Assert.NotNull(queryAsync);
        var testClasses = queryAsync as TestClass[] ?? queryAsync.ToArray();
        Assert.NotEmpty(testClasses);
        Assert.Equal(123, testClasses[0].Id);
    }

    [Fact]
    public async Task QueryDynamicParameter_Success()
    {
        await using var connection = await Source.OpenConnectionAsync();
        var queryAsync =
            await connection.QueryAsync<TestClass>(@"DECLARE $id AS Int32; SELECT $id as id;",
                new { }.WithParams(("$id", 123)));

        Assert.NotNull(queryAsync);
        var testClasses = queryAsync as TestClass[] ?? queryAsync.ToArray();
        Assert.NotEmpty(testClasses);
        Assert.Equal(123, testClasses[0].Id);
    }

    public class TestClass
    {
        public int Id { get; set; }

        public Guid Uuid { get; set; }
    }

    public DapperTests(LocalDatabaseFixture fixture) : base(fixture)
    {
    }
}

public static class DapperExt
{
    public static DynamicParameters WithParams(this object obj, params (string key, object value)[] pairs)
    {
        if (obj is DynamicParameters @params)
        {
        }
        else
        {
            @params = new DynamicParameters();
        }

        foreach (var (key, value) in pairs) @params.Add(key, value);

        return @params;
    }
}