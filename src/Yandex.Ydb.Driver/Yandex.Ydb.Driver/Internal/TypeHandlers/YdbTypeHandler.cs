using Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;
using Type = System.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers;

public abstract class YdbTypeHandler
{
    public abstract Type GetFieldType(FieldDescription? fieldDescription = null);

    public TAny Read<TAny>(Value value, FieldDescription? fieldDescription = null)
    {
        return this switch
        {
            IYdbSimpleTypeHandler<TAny> simpleTypeHandler => simpleTypeHandler.Read(value, fieldDescription),
            IYdbTypeHandler<TAny> typeHandler => typeHandler.Read(value, fieldDescription),
            IYdbJsonTypeHandler jsonTypeHandler => jsonTypeHandler.ReadJson<TAny>(value, fieldDescription),
            _ => throw new NotSupportedException(
                $"Handler `{this.GetType().Name}` does not implement reading for type `{typeof(TAny).Name}`")
        };
    }

    public void Write<TAny>(TAny? value, Value dest)
    {
        switch (this)
        {
            case IYdbSimpleTypeHandler<TAny> handler:
            {
                handler.Write(value, dest);
                break;
            }
            case IYdbTypeHandler<TAny> handler:
            {
                handler.Write(value, dest);
                break;
            }
            default:
                throw new NotSupportedException(
                    $"Writing `{typeof(TAny).Name}` does not supported by `{GetType().Name}` handler");
        }
    }

    public abstract object? ReadAsObject(Value value, FieldDescription? fieldDescription = null);

    public abstract global::Ydb.Type GetYdbType<TAny>(TAny? value);
}

public abstract class YdbTypeHandler<TDefault> : YdbTypeHandler, IYdbTypeHandler<TDefault>
{
    public abstract TDefault Read(Value value, FieldDescription? fieldDescription = null);

    public abstract void Write(TDefault value, Value dest);

    public override object? ReadAsObject(Value value, FieldDescription? fieldDescription = null)
    {
        return Read(value, fieldDescription);
    }

    public override global::Ydb.Type GetYdbType<TAny>(TAny? value) where TAny : default
    {
        if (value is null)
        {
            var type = new global::Ydb.Type
            {
                OptionalType = new OptionalType()
                {
                    Item = GetYdbTypeInternal(value)
                }
            };
            return type;
        }

        return GetYdbTypeInternal(value);
    }

    protected abstract global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value);

    public override Type GetFieldType(FieldDescription? fieldDescription = null)
    {
        return typeof(TDefault);
    }
}