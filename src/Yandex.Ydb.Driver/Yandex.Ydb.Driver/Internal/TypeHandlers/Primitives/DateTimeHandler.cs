using Yandex.Ydb.Driver.Helpers;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class DateTimeHandler : YdbTypeHandler<DateTime>, IYdbSimpleTypeHandler<DateTime>
{
    public override DateTime Read(Value value, FieldDescription? fieldDescription = null)
    {
        return DateTimeOffset.FromUnixTimeSeconds(value.GetInt32()).DateTime;
    }

    public override void Write(DateTime value, Value dest)
    {
        dest.Int64Value = new DateTimeOffset(value).ToUnixTimeSeconds();
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Datetime
        };
}