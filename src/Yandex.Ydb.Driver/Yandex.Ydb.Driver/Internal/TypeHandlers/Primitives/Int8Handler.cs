using Yandex.Ydb.Driver.Helpers;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class Int8Handler : YdbPrimitiveTypeHandler<byte>
{
    public override void Write(byte value, Value dest)
    {
        dest.Int32Value = value;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Int8
        };

    protected override void WriteAsObject(object value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }
    
    public override void Write(bool value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override void Write(int value, Value dest)
    {
        dest.Int32Value = value;
    }

    public override void Write(long value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override void Write(sbyte value, Value dest)
    {
        dest.Int32Value = value;
    }

    public override void Write(short value, Value dest)
    {
        dest.Int32Value = value;
    }

    public override void Write(string value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override void Write(uint value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override void Write(ulong value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override void Write(ushort value, Value dest)
    {
        dest.Int32Value = Convert.ToByte(value);
    }

    public override object ReadAsObject(Value value, FieldDescription? fieldDescription = null)
    {
        return value.GetByte();
    }
}