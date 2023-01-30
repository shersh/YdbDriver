using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Type = System.Type;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public sealed class TypeMapper
{
    private readonly ConcurrentDictionary<Type, YdbTypeHandler> _handlersByClrType = new();
    private readonly ConcurrentDictionary<string, YdbTypeHandler> _handlersByDataTypeName = new();

    private readonly ConcurrentDictionary<uint, YdbTypeHandler> _handlersByOID = new();

    private readonly Dictionary<uint, YdbTypeHandler> _userTypeMappings = new();
    private readonly object _writeLock = new();
    private ILogger _logger;

    private volatile TypeHandlerResolver[] _resolvers;

    #region Construction

    internal TypeMapper()
    {
        UnrecognizedTypeHandler = new UnknownTypeHandler();
        _resolvers = Array.Empty<TypeHandlerResolver>();
    }

    #endregion Constructors

    internal YdbTypeHandler UnrecognizedTypeHandler { get; }

    internal void Initialize(
        IReadOnlyList<TypeHandlerResolverFactory> resolverFactories,
        IReadOnlyDictionary<string, IUserTypeMapping> userTypeMappings, ILogger logger)
    {
        _logger = logger;
        var resolvers = new TypeHandlerResolver[resolverFactories.Count];
        for (var i = 0; i < resolverFactories.Count; i++)
            resolvers[i] = resolverFactories[i].Create();

        _resolvers = resolvers;

        foreach (var userTypeMapping in userTypeMappings.Values)
        {
            //TODO
        }
    }

    public YdbTypeHandler ResolveByYdbType(global::Ydb.Type type)
    {
        lock (_writeLock)
        {
            foreach (var resolver in _resolvers)
                try
                {
                    if (resolver.ResolveByYdbType(type) is YdbTypeHandler { } handler)
                        return handler;
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Type resolver `{Name}` threw exception while resolving value with type `{YdbType}`",
                        resolver.GetType().Name, type.TypeCase);
                }
        }

        throw new NotSupportedException();
    }

    #region Type handler lookup

    internal YdbTypeHandler ResolveByOID(uint oid)
    {
        return TryResolveByOID(oid, out var result) ? result : UnrecognizedTypeHandler;
    }

    internal bool TryResolveByOID(uint oid, [NotNullWhen(true)] out YdbTypeHandler? handler)
    {
        if (_handlersByOID.TryGetValue(oid, out handler))
            return true;

        handler = null;
        return false;
    }


    internal YdbTypeHandler ResolveByDataTypeName(string typeName)
    {
        return ResolveByDataTypeNameCore(typeName)!;
    }

    private YdbTypeHandler? ResolveByDataTypeNameCore(string typeName)
    {
        if (_handlersByDataTypeName.TryGetValue(typeName, out var handler))
            return handler;

        return ResolveLong(typeName);

        YdbTypeHandler? ResolveLong(string typeName)
        {
            lock (_writeLock)
            {
                foreach (var resolver in _resolvers)
                    try
                    {
                        if (resolver.ResolveByDataTypeName(typeName) is { } handler)
                            return _handlersByDataTypeName[typeName] = handler;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,
                            "Type resolver {Name} threw exception while resolving data type name {Type}",
                            resolver.GetType().Name, typeName);
                    }

                return null;
            }
        }
    }

    internal YdbTypeHandler ResolveByValue<T>(T? value)
    {
        if (value is null)
            return ResolveByClrType(typeof(T));

        if (typeof(T).IsValueType)
        {
            YdbTypeHandler? handler;

            lock (_writeLock)
            {
                foreach (var resolver in _resolvers)
                    try
                    {
                        if ((handler = resolver.ResolveValueTypeGenerically(value)) is not null)
                            return handler;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,
                            "Type resolver `{Name}` threw exception while resolving value with type `{Type}`",
                            resolver.GetType().Name, typeof(T));
                    }
            }
        }

        return ResolveByValue((object)value);
    }

    internal YdbTypeHandler ResolveByValue(object? value)
    {
        if (value == null)
        {
            // returns NullHandler
            throw new NotImplementedException("Returns here null handler");
        }

        var type = value.GetType();
        if (_handlersByClrType.TryGetValue(type, out var handler))
            return handler;

        return ResolveLong(value, type);

        YdbTypeHandler ResolveLong(object value, Type type)
        {
            foreach (var resolver in _resolvers)
                try
                {
                    if (resolver.ResolveValueDependentValue(value) is { } handler)
                        return handler;
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Type resolver {Name} threw exception while resolving value with type {Type}",
                        resolver.GetType().Name, type);
                }

            return ResolveByClrType(type);
        }
    }

    internal YdbTypeHandler ResolveByClrType(Type type)
    {
        if (_handlersByClrType.TryGetValue(type, out var handler))
            return handler;

        lock (_writeLock)
        {
            foreach (var resolver in _resolvers)
                try
                {
                    if ((handler = resolver.ResolveByClrType(type)) is not null)
                        return _handlersByClrType[type] = handler;
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Type resolver {Name} threw exception while resolving value with type {Type}",
                        resolver.GetType().Name, type);
                }

            throw new NotSupportedException($"Type `{type.Name}` is not supported by any resolver");
        }
    }

    #endregion Type handler lookup
}

internal sealed class UnknownTypeHandler : TextTypeHandler
{
}