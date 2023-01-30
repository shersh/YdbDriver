using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class DateOnlyHandler : YdbTypeHandler<DateTime>,
    IYdbSimpleTypeHandler<string>, IYdbSimpleTypeHandler<DateTime>
{
    public override DateTime Read(Value value, FieldDescription? fieldDescription = null)
    {
        return DateTimeOffset.UnixEpoch.AddDays(value.GetUInt32()).Date;
    }

    public void Write(string value, Value dest)
    {
        throw new NotImplementedException();
    }

    public override void Write(DateTime value, Value dest)
    {
        dest.Uint32Value = (uint)(value - DateTimeOffset.UnixEpoch).TotalDays;
    }

    string IYdbTypeHandler<string>.Read(Value value, FieldDescription? fieldDescription)
    {
        return DateTimeOffset.UnixEpoch.AddDays(value.GetUInt32()).Date.ToString("dd.MM.yyyy");
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Date
        };
}