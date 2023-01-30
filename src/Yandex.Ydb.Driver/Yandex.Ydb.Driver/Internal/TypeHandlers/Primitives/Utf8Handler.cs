using Google.Protobuf;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class Utf8Handler : YdbTypeHandler<string>, IYdbSimpleTypeHandler<string>,
    IYdbSimpleTypeHandler<Utf8String>
{
    public override string Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.TextValue;
    }

    public void Write(Utf8String value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFrom(value.Value.AsSpan());
    }

    public override void Write(string value, Value dest)
    {
        dest.TextValue = value;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Utf8
        };

    Utf8String IYdbTypeHandler<Utf8String>.Read(Value value, FieldDescription? fieldDescription)
    {
        return new Utf8String(value.GetString());
    }
}