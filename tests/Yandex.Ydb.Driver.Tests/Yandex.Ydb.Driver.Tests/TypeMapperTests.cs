using Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Tests;

public class TypeMapperTests
{
    [Fact]
    public void Int64Handler_WriteInt64Value()
    {
        var tv = new TypedValue()
        {
            Type = new Type() { TypeId = Type.Types.PrimitiveTypeId.Int32 },
            Value = new Value() { }
        };

        var handler = new Int64Handler();
        handler.Write(321321, tv.Value);
        Assert.Equal(321321, tv.Value.Int64Value);
        
        handler.Write("1111", tv.Value);
        Assert.Equal(1111, tv.Value.Int64Value);
        
        handler.Write(true, tv.Value);
        Assert.Equal(1, tv.Value.Int64Value);
        
        handler.Write(1.0, tv.Value);
        Assert.Equal(1, tv.Value.Int64Value);
    }
    
    [Fact]
    public void Int32Handler_writeIntValue()
    {
        var tv = new TypedValue()
        {
            Type = new Type() { TypeId = Type.Types.PrimitiveTypeId.Int32 },
            Value = new Value() { }
        };

        var handler = new Int32Handler();
        handler.Write(321321, tv.Value);
        Assert.Equal(321321, tv.Value.Int32Value);
        
        handler.Write("1111", tv.Value);
        Assert.Equal(1111, tv.Value.Int32Value);
        
        handler.Write(true, tv.Value);
        Assert.Equal(1, tv.Value.Int32Value);
    }

    [Fact]
    public void Int32Handler_ReturnsIntValue()
    {
        var tv = new TypedValue()
        {
            Type = new Type() { TypeId = Type.Types.PrimitiveTypeId.Int32 },
            Value = new Value() { Int32Value = 123123 }
        };

        var handler = new Int32Handler();
        var read = handler.Read<int>(tv.Value);
        Assert.Equal(123123, read);

        var readLong = handler.Read<long>(tv.Value);
        Assert.Equal(123123L, readLong);

        var readString = handler.Read<string>(tv.Value);
        Assert.Equal("123123", readString);
    }
}