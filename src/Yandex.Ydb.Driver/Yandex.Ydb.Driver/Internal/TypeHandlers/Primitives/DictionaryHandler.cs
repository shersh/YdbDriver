using System.Collections;
using System.Diagnostics;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class DictionaryHandler : ContainerHandlerBase<IDictionary>
{
    public override IDictionary Read(Value value, FieldDescription? fieldDescription = null)
    {
        var keyType = fieldDescription.Type.DictType.Key;
        var valueType = fieldDescription.Type.DictType.Payload;

        var keyHandler = Mapper.ResolveByYdbType(keyType);
        var valueHandler = Mapper.ResolveByYdbType(valueType);

        var dictType = typeof(Dictionary<,>).MakeGenericType(keyHandler.GetFieldType(), valueHandler.GetFieldType());
        var dict = (IDictionary)Activator.CreateInstance(dictType)!;

        foreach (var item in value.Pairs)
        {
            var key = item.Key;
            var payload = item.Payload;

            dict.Add(keyHandler.ReadAsObject(key)!, valueHandler.ReadAsObject(payload));
        }

        return dict;
    }

    public override void Write(IDictionary value, Value dest)
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        YdbTypeHandler? keyHandler = null;
        YdbTypeHandler? valueHandler = null;
        foreach (DictionaryEntry kv in value)
        {
            keyHandler ??= Mapper.ResolveByValue(kv.Key);
            valueHandler ??= Mapper.ResolveByValue(kv.Value);

            var key = new Value();
            var val = new Value();

            keyHandler.Write(kv.Key, key);
            valueHandler.Write(kv.Value, val);

            dest.Pairs.Add(new ValuePair()
            {
                Key = key,
                Payload = val
            });
        }
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var type = typeof(TDefault);
        var key = type.GetGenericArguments()[0];
        var payload = type.GetGenericArguments()[1];

        return new global::Ydb.Type
        {
            DictType = new DictType()
            {
                Key = Mapper.ResolveByClrType(key).GetYdbType(value),
                Payload = Mapper.ResolveByClrType(payload).GetYdbType(value),
            }
        };
    }

    public override TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription)
    {
        var dictType = typeof(TAny);
        var genericArguments = dictType.GetGenericArguments();

        var keyType = genericArguments[0];
        var valueType = genericArguments[1];

        var keyHandler = Mapper.ResolveByClrType(keyType);
        var valueHandler = Mapper.ResolveByClrType(valueType);

        var dict = (IDictionary)Activator.CreateInstance(dictType)!;

        foreach (var item in value.Pairs)
        {
            var key = item.Key;
            var payload = item.Payload;

            dict.Add(keyHandler.ReadAsObject(key)!, valueHandler.ReadAsObject(payload));
        }

        return (TAny)dict;
    }

    public override void WriteContainer<TAny>(TAny value, Value dest)
    {
        Write((value as IDictionary)!, dest);
    }
}