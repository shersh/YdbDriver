using System.Collections;
using System.Diagnostics;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class ListHandler : ContainerHandlerBase<IEnumerable>
{
    public override IEnumerable Read(Value value, FieldDescription? fieldDescription = null)
    {
        var handlerItem = Mapper!.ResolveByYdbType(fieldDescription!.Type.ListType.Item);
        var fieldType = handlerItem.GetFieldType();
        var listType = typeof(List<>).MakeGenericType(fieldType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var fd = new FieldDescription(fieldDescription.Type.ListType.Item, string.Empty, 0, Mapper);
        foreach (var item in value.Items)
        {
            var asObject = handlerItem.ReadAsObject(item, fd);
            list.Add(asObject);
        }

        return list;
    }

    public override TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription)
    {
        var handlerItem = Mapper!.ResolveByYdbType(fieldDescription!.Type.ListType.Item);
        var type = typeof(TAny);
        if (type.IsAbstract || type.IsInterface) return (TAny)Read(value, fieldDescription);

        var collection = Activator.CreateInstance(type);
        if (collection is not IList list)
        {
            if (collection is not IEnumerable)
                throw new NotSupportedException($"Type `{type.Name}` is not supported by `{GetType().Name}` mapper");

            return (TAny)Read(value, fieldDescription);
        }

        var fd = new FieldDescription(fieldDescription.Type.ListType.Item, string.Empty, 0, Mapper);
        foreach (var item in value.Items)
        {
            var asObject = handlerItem.ReadAsObject(item, fd);
            list.Add(asObject);
        }

        return (TAny)collection;
    }

    public override void WriteContainer<TAny>(TAny value, Value dest)
    {
        Write((value as IEnumerable)!, dest);
    }

    public override void Write(IEnumerable value, Value dest)
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        YdbTypeHandler? handler = null;
        foreach (var o in value)
        {
            handler ??= Mapper.ResolveByValue(o);
            var nestValue = new Value();
            handler.Write(o, nestValue);
            dest.Items.Add(nestValue);
        }
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");
        var type = typeof(TDefault);
        var definition = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];

        return new Type
        {
            ListType = new ListType
            {
                Item = Mapper.ResolveByClrType(definition)
                    .GetYdbType(value)
            }
        };
    }
}