using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Tests;

public class CustomMappingTests : BaseTests
{
    public CustomMappingTests(LocalDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void AddedCustomMapping_UsingDuringReading()
    {
        GlobalTypeMapper.Instance.UserTypeMappings.TryAdd("TestStructClass", new TestStructClassMapping());

        var yql = @"
SELECT AsStruct(
    123 AS Index,
    ""TestName"" AS b    
  ) AS `struct`;";

        var dbCommand = Source.CreateCommand(yql);
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