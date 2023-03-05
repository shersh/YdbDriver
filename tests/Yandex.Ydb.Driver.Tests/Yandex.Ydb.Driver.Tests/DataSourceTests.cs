using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Tests;

public class CustomMappingTests
{
    [Fact]
    public void AddedCustomMapping_UsingDuringReading()
    {
        GlobalTypeMapper.Instance.UserTypeMappings.TryAdd("TestStructClass", new TestStructClassMapping());

        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsStruct(
    123 AS Index,
    ""TestName"" AS b    
  ) AS `struct`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<TestStructClass>(0);
        Assert.NotNull(lsit);

        Assert.Equal(new TestStructClass(123, "TestName"), lsit);
    }

    public class TestStructClassMapping : IUserTypeMapping
    {
        public Type YdbType { get; } = new() { StructType = new StructType() };
        public System.Type ClrType => typeof(TestStructClass);

        public YdbTypeHandler CreateHandler()
        {
            return new TestStructClassHandler();
        }
    }

    public class TestStructClassHandler : YdbTypeHandler<TestStructClass>
    {
        public override TestStructClass Read(Value value, FieldDescription? fieldDescription = null)
        {
            return new TestStructClass(value.Items[0].GetInt32(), value.Items[1].GetString());
        }

        public override void Write(TestStructClass value, Value dest)
        {
            throw new NotImplementedException();
        }

        protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
        {
            return new Type { StructType = new StructType() };
        }
    }

    public record TestStructClass(int Index, string? Name);
}

public class DataSourceTests
{
    [Fact]
    public void TestSelectAsStructWithNestedStruct_ReturnsDictionaryWithDictionary()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsStruct(
    1 AS a,
    2 AS b,
    AsStruct(1 as a) AS c
  ) AS `struct`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
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
    public void TestSelectAsStruct_ReturnsDictionary()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsStruct(
    1 AS a,
    2 AS b,
    ""3"" AS c
  ) AS `struct`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
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
    public void TestWriteTuple_AcceptsTuple()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
DECLARE $tuple AS Tuple<String, Int32>;

SELECT $tuple AS `tuple`;";

        var dbCommand = dataSource.CreateCommand(yql);
        dbCommand.AddParameter("$tuple", new Tuple<string, int>("a", 1));
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Tuple<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Tuple<string, int>("a", 1), lsit);
    }

    [Fact]
    public void TestSelectAsTuple_ReturnsTuple()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsTuple(""a"", 1) AS `tuple`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Tuple<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Tuple<string, int>("a", 1), lsit);
    }


    [Fact]
    public void TestWriteDict_AcceptsDict()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
DECLARE $lst as Dict<String, Int32>;
SELECT $lst AS `dict`;";

        var dbCommand = dataSource.CreateCommand(yql);
        dbCommand.AddParameter("$lst", new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } });
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } }, lsit);
    }

    [Fact(Skip = "CURRENTLY IT'S NOT WORKING IN YDB AND RETURNS ERROR INSTEAD OF CORRECT RESULT. WAIT FOR FIX")]
    public void TestSelectAsDictWithoutParameters_ReturnsDictionary()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsDict() AS `dict`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int>(), lsit);
    }

    [Fact]
    public void TestSelectConcreteEmptyDict_ReturnsDictionary()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT DictCreate(String, Int32) AS `dict`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int>(), lsit);
    }

    [Fact]
    public void TestSelectAsDict_ReturnsDictionary()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
SELECT AsDict(
    AsTuple(""a"", 1),
    AsTuple(""b"", 2),
    AsTuple(""c"", 3)
  ) AS `dict`;";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<Dictionary<string, int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } }, lsit);
    }

    [Fact]
    public void TestWriteList_AcceptsArray()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
DECLARE $lst as List<Int32>;
SELECT $lst AS `list`;";

        var dbCommand = dataSource.CreateCommand(yql);
        dbCommand.AddParameter("$lst", new[] { 1, 2, 3 });
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }

    [Fact]
    public void TestWriteList_AcceptsList()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"
DECLARE $lst as List<Int32>;
SELECT $lst AS `list`;";

        var dbCommand = dataSource.CreateCommand(yql);
        dbCommand.AddParameter("$lst", new List<int> { 1, 2, 3 });
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }

    [Fact]
    public void TestSelectList_ReturnsList()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"SELECT
  AsList(1, 2, 3) AS `list`";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);


        var enumerable = reader.GetFieldValue<IEnumerable<int>>(0);
        Assert.NotNull(enumerable);
        Assert.Equal(new[] { 1, 2, 3 }, enumerable);

        var lsit = reader.GetFieldValue<List<int>>(0);
        Assert.NotNull(lsit);
        Assert.Equal(new[] { 1, 2, 3 }, lsit);
    }


    [Fact]
    public void TestDoubleList()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");

        var yql = @"SELECT
                AsList(AsList(1, 2, 3), AsList(1, 2, 3)) AS `list`";

        var dbCommand = dataSource.CreateCommand(yql);
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
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
    public void DataSourceWithSsl_NullCertPath_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });

        Assert.IsType<InvalidDataException>(exception.InnerException);
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
    public void DataSourceWithSsl_IsWorking()
    {
        using var dataSource =
            YdbDataSource.Create(
                "Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");
        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public void UnpooledYdbDataSource_CreateCommand_Success()
    {
        using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=false");

        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public void PooledYdbDataSource_CreateCommand_Success()
    {
        using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");

        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public async Task PooledYdbDataSource_IsPolledDataSource_True()
    {
        await using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");
        var connection = await dataSource.OpenConnectionAsync();
        Assert.Equal((1, 0, 0), dataSource.Statistics);
        await connection.CloseAsync();
        Assert.Equal((1, 1, 1), dataSource.Statistics);
    }

    [Fact]
    public async Task PooledYdbDataSource_OpenThreeConnection_SuccessStatistics()
    {
        await using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");
        var connection1 = await dataSource.OpenConnectionAsync();
        Assert.Equal((1, 0, 0), dataSource.Statistics);
        var connection2 = await dataSource.OpenConnectionAsync();
        Assert.Equal((2, 0, 0), dataSource.Statistics);
        var connection3 = await dataSource.OpenConnectionAsync();
        Assert.Equal((3, 0, 0), dataSource.Statistics);
        connection1.Close();
        Assert.Equal((3, 1, 1), dataSource.Statistics);
        connection2.Close();
        Assert.Equal((3, 2, 2), dataSource.Statistics);
        connection3.Close();
        Assert.Equal((3, 3, 3), dataSource.Statistics);
        var connection4 = await dataSource.OpenConnectionAsync();
        Assert.Equal((3, 2, 2), dataSource.Statistics);
    }
}