using System.Data;
using System.Data.Common;
using System.Globalization;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb.Table;

namespace Yandex.Ydb.Driver.Tests;

public class YdbCommandTests
{
    [Fact]
    public async Task ExecuteReader_WithJsonParameter_ReturnsJson()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();
        command.TxControl = null;
        command.CommandText = "DECLARE $obj as Json; SELECT $obj;";

        command.AddParameter("$obj", JsonValue.From(new { Hello = "From YDB ;)", Index = 123 }));

        var reader = await command.ExecuteReaderAsync();
        Assert.NotNull(reader);
        var isRead = await reader.ReadAsync();
        Assert.True(isRead);

        var str = reader.GetString(0);
        Assert.Equal(@"{""Hello"":""From YDB ;)"",""Index"":123}", str);
        var fieldValue = reader.GetFieldValue<JsonValue>(0);
        Assert.Equal(@"{""Hello"":""From YDB ;)"",""Index"":123}", fieldValue.Value);

        var testClass = reader.GetFieldValue<JsonTestClass>(0);
        Assert.Equal(123, testClass.Index);
        Assert.Equal("From YDB ;)", testClass.Hello);
    }


    [Fact]
    public void ExecuteNonQuery_WithDefaultTxControl_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();
        command.TxControl = null;
        command.CommandText = "SELECT 1;";
        var result = command.ExecuteNonQuery();

        Assert.Equal(1, result);
    }

    [Fact]
    public void ExecuteNonQuery_WithoutTransaction_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();
        command.TxControl = new TransactionControl
        {
            BeginTx = new TransactionSettings { SerializableReadWrite = new SerializableModeSettings() }
        };
        command.CommandText = "SELECT 1;";
        var result = command.ExecuteNonQuery();

        Assert.Equal(1, result);
    }

    [Fact]
    public void ExecuteNonQuery_InsideTransaction_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var command = connection.CreateYdbCommand();
        command.CommandText = "SELECT 1;";
        command.Transaction = transaction;

        var result = command.ExecuteNonQuery();

        transaction.Commit();
    }

    [Fact]
    public void ExecuteCommandWithGenericParameters_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();

        command.CommandText = @"
            DECLARE $id AS Int32;
            DECLARE $count AS Uint64;
            DECLARE $some AS Uint32;
            DECLARE $b AS Bool;
            SELECT $id,$count,$some,$b;";

        var dbParameter = command.CreateParameter();

        dbParameter.ParameterName = "$id";
        dbParameter.Value = -321;

        command.Parameters.Add(dbParameter);
        command.Parameters.Add(new YdbParameter<ulong>("$count", 89172318273));
        command.Parameters.Add(new YdbParameter<uint>("$some", 123123u));
        command.Parameters.Add(new YdbParameter<bool>("$b", true));

        var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(-321, reader.GetInt32(0));
        Assert.Equal(89172318273, reader.GetInt64(1));
        Assert.Equal(123123, reader.GetInt32(2));
        Assert.Equal(true, reader.GetBoolean(3));
    }

    [Fact]
    public void ExecuteCommandWithParameters_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();

        command.CommandText = @"
            DECLARE $id AS Int32;
            DECLARE $count AS Uint64;
            DECLARE $some AS Uint32;
            DECLARE $b AS Bool;
            SELECT $id,$count,$some,$b;";

        var dbParameter = command.CreateParameter();

        dbParameter.ParameterName = "$id";
        dbParameter.Value = -321;

        command.Parameters.Add(dbParameter);
        command.Parameters.Add(new YdbParameter("$count", (ulong)89172318273));
        command.Parameters.Add(new YdbParameter("$some", 123123u));
        command.Parameters.Add(new YdbParameter("$b", true));

        var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(-321, reader.GetInt32(0));
        Assert.Equal(89172318273, reader.GetInt64(1));
        Assert.Equal(123123, reader.GetInt32(2));
        Assert.Equal(true, reader.GetBoolean(3));
    }

    [Fact]
    public void ExecuteDbDataReader_Success()
    {
        var connection = TestHelper.GetDefaultConnectionAndOpen();
        var command = connection.CreateYdbCommand();

        var examples = new List<(object, Func<DbDataReader, int, object>)>
        {
            (true, (dataReader, i) => dataReader.GetBoolean(i)),
            ((short)1, (dataReader, i) => dataReader.GetInt16(i)),
            (-1, (dataReader, i) => dataReader.GetInt32(i)),
            (2, (dataReader, i) => dataReader.GetInt32(i)),
            (-3L, (dataReader, i) => dataReader.GetInt64(i)),
            (4L, (dataReader, i) => dataReader.GetInt64(i)),
            (-5f, (dataReader, i) => dataReader.GetFloat(i)),
            (6d, (dataReader, i) => dataReader.GetDouble(i)),
            ("foo", (dataReader, i) => dataReader.GetString(i)),
            ("Hello", (dataReader, i) => dataReader.GetString(i)),
            (Guid.Parse("f9d5cc3f-f1dc-4d9c-b97e-766e57ca4ccb"), (dataReader, i) => dataReader.GetGuid(i)),
            ("Im Optional", (dataReader, i) => dataReader.GetString(i)),
            ("Im Optional twice or more", (dataReader, i) => dataReader.GetString(i)),
            ((decimal)1.23, (dataReader, i) => dataReader.GetDecimal(i)),
            ("<a=1>[3;%false]", (dataReader, i) => dataReader.GetString(i)),
            (@"{""a"":1,""b"":null}", (dataReader, i) => dataReader.GetString(i)),
            (new DateTime(2017, 11, 27), (dataReader, i) => dataReader.GetFieldValue<DateTime>(i)),
            ("27.11.2017", (dataReader, i) => dataReader.GetString(i)),
            (new DateTime(2017, 11, 27, 13, 24, 0), (dataReader, i) => dataReader.GetFieldValue<DateTime>(i)),
            (DateTimeOffset.Parse("2017-11-27T13:24:00.123456Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                (dataReader, i) => dataReader.GetFieldValue<DateTimeOffset>(i)),
            (DateTime.Parse("2017-11-27T13:24:00.123456Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                (dataReader, i) => dataReader.GetFieldValue<DateTime>(i)),

            (new TestClass(1, null), (dataReader, i) => dataReader.GetFieldValue<TestClass>(i))
        };

        command.CommandText = @"SELECT
            Bool('true'),
            Uint8('1'),
            Int32('-1'),
            Uint32('2'),
            Int64('-3'),
            Uint64('4'),
            Float('-5'),
            Double('6'),
            String('foo'),
            Utf8('Hello'),
            Uuid('f9d5cc3f-f1dc-4d9c-b97e-766e57ca4ccb'),
            CAST(Utf8('Im Optional') AS Optional<Utf8>),
            CAST(Utf8('Im Optional twice or more') AS Optional<Optional<Optional<Utf8>>>),
            Decimal('1.23', 5, 2), -- up to 5 decimal digits, with 2 after the decimal point
            Yson('<a=1>[3;%false]'),
            Json(@@{""a"":1,""b"":null}@@),
            Date('2017-11-27'),
            Date('2017-11-27'),
            Datetime('2017-11-27T13:24:00Z'),
            Timestamp('2017-11-27T13:24:00.123456Z'),
            Timestamp('2017-11-27T13:24:00.123456Z'),
            Json(@@{""a"":1,""b"":null}@@),

            Interval('P1DT2H3M4.567890S'), -- NOT IMPLEMENTED YET
            TzDate('2017-11-27,Europe/Moscow'), -- NOT IMPLEMENTED YET
            TzDatetime('2017-11-27T13:24:00,America/Los_Angeles'), -- NOT IMPLEMENTED YET
            TzTimestamp('2017-11-27T13:24:00.123456,GMT') -- NOT IMPLEMENTED YET";
        var reader = command.ExecuteReader();

        Assert.NotNull(reader);
        while (reader.Read())
        {
            var i = 0;
            foreach (var (testValue, func) in examples)
            {
                var res = func(reader, i++);
                Assert.Equal(testValue, res);
            }

            return;
        }

        Assert.Fail("Should return before");
    }

    public class JsonTestClass
    {
        public string Hello { get; set; }

        public int Index { get; set; }
    }

    public record TestClass(int a, string? b);
}