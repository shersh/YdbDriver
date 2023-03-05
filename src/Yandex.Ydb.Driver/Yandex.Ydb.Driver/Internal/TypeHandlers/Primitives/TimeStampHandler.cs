using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class TimeStampHandler : YdbTypeHandler<DateTimeOffset>, IYdbSimpleTypeHandler<DateTimeOffset>,
    IYdbSimpleTypeHandler<DateTime>, IYdbSimpleTypeHandler<Timestamp>
{
    public void Write(DateTime value, Value dest)
    {
        dest.Int64Value = new DateTimeOffset(value).ToUnixTimeMilliseconds();
    }

    DateTime IYdbTypeHandler<DateTime>.Read(Value value, FieldDescription? fieldDescription)
    {
        return DateTimeOffset.UnixEpoch.AddTicks(value.GetInt64() * 10).DateTime;
    }

    public override DateTimeOffset Read(Value value, FieldDescription? fieldDescription = null)
    {
        return DateTimeOffset.UnixEpoch.AddTicks(value.GetInt64() * 10);
    }

    public override void Write(DateTimeOffset value, Value dest)
    {
        dest.Int64Value = value.ToUnixTimeMilliseconds();
    }

    public void Write(Timestamp value, Value dest)
    {
        dest.Int64Value = value.Value;
    }

    Timestamp IYdbTypeHandler<Timestamp>.Read(Value value, FieldDescription? fieldDescription)
    {
        return new Timestamp(value.GetInt64());
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.Timestamp
        };
    }
}