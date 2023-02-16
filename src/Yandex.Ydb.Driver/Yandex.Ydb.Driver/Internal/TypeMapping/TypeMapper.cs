using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Type = System.Type;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public sealed class TypeMapper
{
    private readonly ConcurrentDictionary<Type, YdbTypeHandler> _handlersByClrType = new();
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
        var resolvers = new List<TypeHandlerResolver> { new UserDefinedTypeHandlerResolver(userTypeMappings.Values) };
        resolvers.AddRange(resolverFactories.Select(factory => factory.Create()));
        _resolvers = resolvers.ToArray();
    }

    public YdbTypeHandler ResolveByYdbType(global::Ydb.Type type)
    {
        foreach (var resolver in _resolvers)
            try
            {
                if (resolver.ResolveByYdbType(type) is not YdbTypeHandler { } handler)
                    continue;

                if (handler is IContainerHandler { } containerHandler)
                    containerHandler.SetMapper(this);

                return handler;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Type resolver `{Name}` threw exception while resolving value with type `{YdbType}`",
                    resolver.GetType().Name, type.TypeCase);
            }

        throw new NotSupportedException();
    }

    #region Type handler lookup

    internal YdbTypeHandler ResolveByValue<T>(T? value)
    {
        if (value is null)
            return ResolveByClrType(typeof(T));

        if (typeof(T).IsValueType)
        {
            YdbTypeHandler? handler;
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

        return ResolveByValue((object)value);
    }

    internal YdbTypeHandler ResolveByValue(object? value)
    {
        if (value == null)
        {
            _logger.LogInformation("Can't resolve handler for null value, returns UnknownTypeHandler");
            return UnrecognizedTypeHandler;
        }

        var type = value.GetType();
        return _handlersByClrType.TryGetValue(type, out var handler) ? handler : ResolveLong(value, type);

        YdbTypeHandler ResolveLong(object resolveValue, Type resolveType)
        {
            foreach (var resolver in _resolvers)
                try
                {
                    if (resolver.ResolveValueDependentValue(resolveValue) is not { } typeHandler)
                        continue;

                    if (typeHandler is IContainerHandler { } containerHandler)
                        containerHandler.SetMapper(this);

                    return typeHandler;
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Type resolver {Name} threw exception while resolving value with type {Type}",
                        resolver.GetType().Name, resolveType);
                }

            return ResolveByClrType(resolveType);
        }
    }

    internal YdbTypeHandler ResolveByClrType(Type type)
    {
        if (_handlersByClrType.TryGetValue(type, out var handler))
            return handler;

        foreach (var resolver in _resolvers)
            try
            {
                if ((handler = resolver.ResolveByClrType(type)) is null)
                    continue;

                if (handler is IContainerHandler { } containerHandler)
                    containerHandler.SetMapper(this);

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

    #endregion Type handler lookup
}

internal sealed class UnknownTypeHandler : TextTypeHandler
{
}