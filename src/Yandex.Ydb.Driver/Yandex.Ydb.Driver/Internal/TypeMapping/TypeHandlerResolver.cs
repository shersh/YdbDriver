using Yandex.Ydb.Driver.Internal.TypeHandlers;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public abstract class TypeHandlerResolver
{
    public abstract YdbTypeHandler? ResolveByClrType(Type type);

    public virtual YdbTypeHandler? ResolveValueDependentValue(object value)
    {
        return null;
    }

    public virtual YdbTypeHandler? ResolveValueTypeGenerically<T>(T value)
    {
        return null;
    }

    public abstract YdbTypeHandler? ResolveByYdbType(global::Ydb.Type type);
}