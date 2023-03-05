using Google.Protobuf;
using Yandex.Ydb.Driver.Helpers;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class YsonHandler : YdbTypeHandler<string>, IYdbSimpleTypeHandler<string>
{
    public override string Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.GetString() ?? string.Empty;
    }

    public override void Write(string value, Value dest)
    {
        dest.BytesValue = ByteString.CopyFromUtf8(value);
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.Yson
        };
    }
}