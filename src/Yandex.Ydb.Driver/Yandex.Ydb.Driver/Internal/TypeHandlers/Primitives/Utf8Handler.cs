using Google.Protobuf;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class Utf8Handler : YdbTypeHandler<string>, IYdbSimpleTypeHandler<string>,
    IYdbSimpleTypeHandler<Utf8String>
{
    public override string Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.TextValue;
    }

    public override void Write(string value, Value dest)
    {
        dest.TextValue = value;
    }

    public void Write(Utf8String value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFrom(value.Value.AsSpan());
    }

    Utf8String IYdbTypeHandler<Utf8String>.Read(Value value, FieldDescription? fieldDescription)
    {
        return new Utf8String(value.GetString());
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.Utf8
        };
    }
}