using System.Collections.Concurrent;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver.Internal.TypeHandling;

internal sealed class UserDefinedTypeHandlerResolver : TypeHandlerResolver
{
    private readonly ConcurrentDictionary<Type, YdbTypeHandler> _handlersByClrType;

    internal UserDefinedTypeHandlerResolver(IEnumerable<IUserTypeMapping>? typeMappings)
    {
        _handlersByClrType = new ConcurrentDictionary<Type, YdbTypeHandler>();
        if (typeMappings != null)
            foreach (var mapping in typeMappings)
                AddTypeMapping(mapping);
    }

    internal void AddTypeMapping(IUserTypeMapping typeMapping)
    {
        _handlersByClrType[typeMapping.ClrType] = typeMapping.CreateHandler();
    }

    public override YdbTypeHandler? ResolveByDataTypeName(string typeName)
    {
        //NOT SUPPORTED
        return null;
    }

    public override YdbTypeHandler? ResolveByClrType(Type type)
    {
        _handlersByClrType.TryGetValue(type, out var handler);
        return handler;
    }

    public override YdbTypeHandler? ResolveByYdbType(global::Ydb.Type type)
    {
        //NOT SUPPORTED
        return null;
    }
}