using Google.Protobuf;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public class TextTypeHandler : YdbPrimitiveTypeHandler<string>, IYdbTypeHandler<Guid>
{
    public Guid Read(Value value, FieldDescription? fieldDescription)
    {
        return Guid.Parse(value.GetString()!);
    }

    public void Write(Guid value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value.ToString()));
    }

    protected override void WriteAsObject(object value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(byte value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.String
        };
    }

    public override void Write(bool value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(int value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(long value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(sbyte value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(short value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(string value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(value);
    }

    public override void Write(uint value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(ulong value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override void Write(ushort value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(Convert.ToString(value));
    }

    public override object ReadAsObject(Value value, FieldDescription? fieldDescription = null)
    {
        return value.GetString();
    }
}