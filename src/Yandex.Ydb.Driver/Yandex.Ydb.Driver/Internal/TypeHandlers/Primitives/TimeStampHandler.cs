using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class ListHandler<T> : YdbTypeHandler<List<T>>
{
    private readonly TypeMapper _mapper;

    public ListHandler(TypeMapper mapper)
    {
        _mapper = mapper;
    }

    public override List<T> Read(Value value, FieldDescription? fieldDescription = null)
    {
        var handlerItem = _mapper.ResolveByYdbType(fieldDescription.Type.ListType.Item);
        var result = new List<T>();
        foreach (var item in value.Items)
        {
            result.Add(handlerItem.Read<T>(item));
        }

        return result;
    }

    public override void Write(List<T> value, Value dest)
    {
        throw new NotImplementedException();
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            ListType = new ListType()
            {
                Item = _mapper.ResolveByValue(value).GetYdbType(value) //TODO: Maybe recursion here? Need investigation 
            }
        };
}

public sealed class TimeStampHandler : YdbTypeHandler<DateTimeOffset>, IYdbSimpleTypeHandler<DateTimeOffset>,
    IYdbSimpleTypeHandler<DateTime>, IYdbSimpleTypeHandler<Timestamp>
{
    public override DateTimeOffset Read(Value value, FieldDescription? fieldDescription = null)
    {
        return DateTimeOffset.UnixEpoch.AddTicks(value.GetInt64() * 10);
    }

    public void Write(Timestamp value, Value dest)
    {
        dest.Int64Value = value.Value;
    }

    public void Write(DateTime value, Value dest)
    {
        dest.Int64Value = new DateTimeOffset(value).ToUnixTimeMilliseconds();
    }

    public override void Write(DateTimeOffset value, Value dest)
    {
        dest.Int64Value = value.ToUnixTimeMilliseconds();
    }

    DateTime IYdbTypeHandler<DateTime>.Read(Value value, FieldDescription? fieldDescription)
    {
        return DateTimeOffset.UnixEpoch.AddTicks(value.GetInt64() * 10).DateTime;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Timestamp
        };

    Timestamp IYdbTypeHandler<Timestamp>.Read(Value value, FieldDescription? fieldDescription)
    {
        return new Timestamp(value.GetInt64());
    }
}