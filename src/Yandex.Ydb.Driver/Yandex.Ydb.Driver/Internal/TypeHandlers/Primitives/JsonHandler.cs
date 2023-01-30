using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public interface IYdbJsonTypeHandler
{
    TAny ReadJson<TAny>(Value value, FieldDescription? fieldDescription);
}

public sealed class JsonHandler : YdbTypeHandler<string>, IYdbTypeHandler<JsonValue>, IYdbSimpleTypeHandler<string>,
    IYdbJsonTypeHandler
{
    public override string Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.GetString() ?? string.Empty;
    }

    public void Write(JsonValue value, Value dest)
    {
        dest.TextValue = value.Value;
    }

    public override void Write(string value, Value dest)
    {
        dest.TextValue = value;
    }

    public TAny ReadJson<TAny>(Value value, FieldDescription? fieldDescription)
    {
        var json = Read(value, fieldDescription);
        return JsonSerializer.Deserialize<TAny>(json)!;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Json
        };

    JsonValue IYdbTypeHandler<JsonValue>.Read(Value value, FieldDescription? fieldDescription)
    {
        return JsonValue.FromString(value.GetString());
    }
}