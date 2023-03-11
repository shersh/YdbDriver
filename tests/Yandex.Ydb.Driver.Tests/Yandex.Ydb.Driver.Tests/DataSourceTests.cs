namespace Yandex.Ydb.Driver.Tests;

public class DataSourceTests : BaseTests
{
    [Fact]
    public async Task TestSelectAsStructWithNestedStruct_ReturnsDictionaryWithDictionary()
    {
        var yql = @"
SELECT AsStruct(
    1 AS a,
    2 AS b,
    AsStruct(1 as a) AS c
  ) AS `struct`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, object?>>(0);
        Assert.NotNull(lsit);

        Assert.Equal(new Dictionary<string, object>
        {
            { "a", 1 },
            { "b", 2 },
            { "c", new Dictionary<string, object> { { "a", 1 } } }
        }, lsit);
    }

    [Fact]
    public async Task TestSelectAsStruct_ReturnsDictionary()
    {
        var yql = @"
SELECT AsStruct(
    1 AS a,
    2 AS b,
    ""3"" AS c
  ) AS `struct`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, object?>>(0);
        Assert.NotNull(lsit);

        Assert.Equal(new Dictionary<string, object>
        {
            { "a", 1 },
            { "b", 2 },
            { "c", "3" }
        }, lsit);
    }

    [Fact]
    public async Task TestWriteTuple_AcceptsTuple()
    {
        var yql = @"
DECLARE $tuple AS Tuple<String, Int32>;

SELECT $tuple AS `tuple`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        dbCommand.AddParameter("$tuple", new Tuple<string, int>("a", 1));
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Tuple<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Tuple<string, int>("a", 1), lsit);
    }

    [Fact]
    public async Task TestSelectAsTuple_ReturnsTuple()
    {
        var yql = @"
SELECT AsTuple(""a"", 1) AS `tuple`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Tuple<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Tuple<string, int>("a", 1), lsit);
    }


    [Fact]
    public async Task TestWriteDict_AcceptsDict()
    {
        var yql = @"
DECLARE $lst as Dict<String, Int32>;
SELECT $lst AS `dict`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        dbCommand.AddParameter("$lst", new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } });
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } }, lsit);
    }

    [Fact(Skip = "CURRENTLY IT'S NOT WORKING IN YDB AND RETURNS ERROR INSTEAD OF CORRECT RESULT. WAIT FOR FIX")]
    public async Task TestSelectAsDictWithoutParameters_ReturnsDictionary()
    {
        var yql = @"
SELECT AsDict() AS `dict`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int>(), lsit);
    }

    [Fact]
    public async Task TestSelectConcreteEmptyDict_ReturnsDictionary()
    {
        var yql = @"
SELECT DictCreate(String, Int32) AS `dict`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();

        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int>(), lsit);
    }

    [Fact]
    public async Task TestSelectAsDict_ReturnsDictionary()
    {
        var yql = @"
SELECT AsDict(
    AsTuple(""a"", 1),
    AsTuple(""b"", 2),
    AsTuple(""c"", 3)
  ) AS `dict`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } }, lsit);
    }

    [Fact]
    public async Task TestWriteList_AcceptsArray()
    {
        var yql = @"
DECLARE $lst as List<Int32>;
SELECT $lst AS `list`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        dbCommand.AddParameter("$lst", new[] { 1, 2, 3 });
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }

    [Fact]
    public async Task TestWriteList_AcceptsList()
    {
        var yql = @"
DECLARE $lst as List<Int32>;
SELECT $lst AS `list`;";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        dbCommand.AddParameter("$lst", new List<int> { 1, 2, 3 });
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }

    [Fact]
    public async Task TestSelectList_ReturnsList()
    {
        var yql = @"SELECT
  AsList(1, 2, 3) AS `list`";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);


        var enumerable = reader.GetFieldValue<IEnumerable<int>>(0);
        Assert.NotNull(enumerable);
        Assert.Equal(new[] { 1, 2, 3 }, enumerable);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }


    [Fact]
    public async Task TestDoubleList()
    {
        var yql = @"SELECT
                AsList(AsList(1, 2, 3), AsList(1, 2, 3)) AS `list`";

        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand(yql);
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);

        var example = new List<List<int>> { new() { 1, 2, 3 }, new() { 1, 2, 3 } };

        var lsit = reader.GetFieldValue<List<List<int>>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(example, lsit);


        var enumerable = reader.GetFieldValue<List<IEnumerable<int>>>(0);
        Assert.NotNull(enumerable);
        Assert.Equal(example, enumerable);
    }

    [Fact]
    public void DataSourceWithSsl_WrongCertPath_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create(
                    "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\wrong_certs;Pooling=true");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });

        Assert.IsType<FileNotFoundException>(exception.InnerException);
    }

    [Fact]
    public void DataSourceWithSsl_CustomCert_AndWithoutTrust_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;Pooling=true");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });
    }

    [Fact]
    public async Task UnpooledYdbDataSource_CreateCommand_Success()
    {
        await using var connection = await Source.OpenConnectionAsync();
        await using var dbCommand = connection.CreateYdbCommand("SELECT 1, Bool('true');");
        await using var reader = await dbCommand.ExecuteReaderAsync();
        var read = await reader.ReadAsync();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    public DataSourceTests(LocalDatabaseFixture fixture) : base(fixture)
    {
    }
}