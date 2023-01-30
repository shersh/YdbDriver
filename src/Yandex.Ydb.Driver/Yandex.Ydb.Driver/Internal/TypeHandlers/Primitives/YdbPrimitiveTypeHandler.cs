using Google.Protobuf.WellKnownTypes;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;
using Type = System.Type;
using Value = Ydb.Value;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public abstract class YdbPrimitiveTypeHandler<TAny> : YdbTypeHandler, IYdbSimpleTypeHandler<bool>,
    IYdbSimpleTypeHandler<int>,
    IYdbSimpleTypeHandler<byte>,
    IYdbSimpleTypeHandler<sbyte>, IYdbSimpleTypeHandler<short>, IYdbSimpleTypeHandler<ushort>,
    IYdbSimpleTypeHandler<long>, IYdbSimpleTypeHandler<ulong>, IYdbSimpleTypeHandler<uint>,
    IYdbSimpleTypeHandler<string>, IYdbSimpleTypeHandler<object>
{
    public override global::Ydb.Type GetYdbType<TDefault>(TDefault? value) where TDefault : default
    {
        if (value is null)
        {
            var type = new global::Ydb.Type
            {
                OptionalType = new OptionalType()
                {
                    Item = GetYdbTypeInternal(value)
                }
            };
            return type;
        }


        return GetYdbTypeInternal(value);
    }

    protected abstract global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value);

    public abstract void Write(bool value, Value dest);

    bool IYdbTypeHandler<bool>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetBool();
    }

    protected abstract void WriteAsObject(object value, Value dest);

    public void Write(object? value, Value dest)
    {
        if (value is null)
            dest.NullFlagValue = NullValue.NullValue;
        else
            WriteAsObject(value, dest);
    }

    public abstract void Write(byte value, Value dest);

    byte IYdbTypeHandler<byte>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetByte();
    }

    int IYdbTypeHandler<int>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetInt32();
    }

    public abstract void Write(int value, Value dest);

    long IYdbTypeHandler<long>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetInt64();
    }

    public abstract void Write(long value, Value dest);
    public abstract void Write(sbyte value, Value dest);

    sbyte IYdbTypeHandler<sbyte>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetSByte();
    }

    public abstract void Write(short value, Value dest);

    short IYdbTypeHandler<short>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetInt16();
    }

    string IYdbTypeHandler<string>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetString() ?? string.Empty;
    }

    public abstract void Write(string value, Value dest);

    uint IYdbTypeHandler<uint>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetUInt32();
    }

    public abstract void Write(uint value, Value dest);

    ulong IYdbTypeHandler<ulong>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetUInt64();
    }

    public abstract void Write(ulong value, Value dest);

    public abstract void Write(ushort value, Value dest);

    ushort IYdbTypeHandler<ushort>.Read(Value value, FieldDescription? fieldDescription)
    {
        return value.GetUInt16();
    }

    public override Type GetFieldType(FieldDescription? fieldDescription = null)
    {
        return typeof(TAny);
    }

    public object Read(Value value, FieldDescription? fieldDescription) => ReadAsObject(value, fieldDescription);
}