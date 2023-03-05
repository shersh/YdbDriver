using Yandex.Ydb.Driver.Helpers;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class BoolHandler : YdbPrimitiveTypeHandler<bool>
{
    public override object ReadAsObject(Value value, FieldDescription? fieldDescription = null)
    {
        return value.GetBool();
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.Bool
        };
    }

    protected override void WriteAsObject(object value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(byte value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(bool value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(int value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(long value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(sbyte value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(short value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(string value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(uint value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(ulong value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }

    public override void Write(ushort value, Value dest)
    {
        dest.BoolValue = Convert.ToBoolean(value);
    }
}