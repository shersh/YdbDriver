using System.Collections;
using Yandex.Cloud.Dns.V1;
using Yandex.Ydb.Driver.Helpers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Yandex.Ydb.Driver.Types.Primitives;
using Ydb;
using Type = System.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public interface IContainerHandler
{
    void SetMapper(TypeMapper mapper);
    TAny ReadEnumerable<TAny>(Value value, FieldDescription? fieldDescription);
}

public sealed class ListHandler : YdbTypeHandler<IEnumerable>, IContainerHandler
{
    private TypeMapper? _mapper;

    public ListHandler()
    {
    }

    public override IEnumerable Read(Value value, FieldDescription? fieldDescription = null)
    {
        var handlerItem = _mapper!.ResolveByYdbType(fieldDescription!.Type.ListType.Item);
        var fieldType = handlerItem.GetFieldType();
        var listType = typeof(List<>).MakeGenericType(fieldType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var fd = new FieldDescription(fieldDescription.Type.ListType.Item, string.Empty, 0, _mapper);
        foreach (var item in value.Items)
        {
            var asObject = handlerItem.ReadAsObject(item, fd);
            list.Add(asObject);
        }

        return list;
    }

    public TAny ReadEnumerable<TAny>(Value value, FieldDescription? fieldDescription)
    {
        var handlerItem = _mapper!.ResolveByYdbType(fieldDescription!.Type.ListType.Item);
        var type = typeof(TAny);
        if (type.IsAbstract || type.IsInterface)
        {
            return (TAny)Read(value, fieldDescription);   
        }
        
        var collection = Activator.CreateInstance(type);
        if (collection is not IList list)
        {
            if (collection is not IEnumerable)
                throw new NotSupportedException($"Type `{type.Name}` is not supported by `{this.GetType().Name}` mapper");
            
            return (TAny)Read(value, fieldDescription);
        }

        var fd = new FieldDescription(fieldDescription.Type.ListType.Item, string.Empty, 0, _mapper);
        foreach (var item in value.Items)
        {
            var asObject = handlerItem.ReadAsObject(item, fd);
            list.Add(asObject);
        }

        return (TAny)collection;
    }

    public override void Write(IEnumerable value, Value dest)
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

    public void SetMapper(TypeMapper mapper)
    {
        _mapper ??= mapper;
    }
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