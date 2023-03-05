using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Yandex.Ydb.Driver.Types.Primitives;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandling;

internal sealed class BuiltInTypeHandlerResolver : TypeHandlerResolver
{
    private static readonly Int32Handler Int32Handler = new();
    private static readonly Int64Handler Int64Handler = new();
    private static readonly UInt64Handler Uint64Handler = new();
    private static readonly Int16Handler Int16Handler = new();
    private static readonly UInt16Handler UInt16Handler = new();
    private static readonly UInt32Handler UInt32Handler = new();
    private static readonly TextTypeHandler TextTypeHandler = new();
    private static readonly Int8Handler Int8Handler = new();
    private static readonly UInt8Handler UInt8Handler = new();
    private static readonly BoolHandler BoolHandler = new();

    private static readonly DoubleHandler DoubleHandler = new();
    private static readonly GuidHandler GuidHandler = new();
    private static readonly FloatHandler FloatHandler = new();
    private static readonly DecimalHandler DecimalHandler = new();
    private static readonly Utf8Handler Utf8Handler = new();

    private static readonly YsonHandler YsonHandler = new();
    private static readonly JsonHandler JsonHandler = new();
    private static readonly DateOnlyHandler DateOnlyHandler = new();
    private static readonly DateTimeHandler DateTimeHandler = new();
    private static readonly TimeStampHandler TimstampHandler = new();

    private static readonly Utf8Handler UnknownHandler = new();

    private static readonly IReadOnlyDictionary<Type.Types.PrimitiveTypeId, YdbTypeHandler> YdbTypeToHandlerTable =
        new Dictionary<Type.Types.PrimitiveTypeId, YdbTypeHandler>
        {
            { Type.Types.PrimitiveTypeId.Int32, Int32Handler },
            { Type.Types.PrimitiveTypeId.Bool, BoolHandler },
            { Type.Types.PrimitiveTypeId.Int8, Int8Handler },
            { Type.Types.PrimitiveTypeId.Uint8, UInt8Handler },
            { Type.Types.PrimitiveTypeId.Int16, Int16Handler },
            { Type.Types.PrimitiveTypeId.Uint16, UInt16Handler },
            { Type.Types.PrimitiveTypeId.Uint32, UInt32Handler },
            { Type.Types.PrimitiveTypeId.Int64, Int64Handler },
            { Type.Types.PrimitiveTypeId.Uint64, Uint64Handler },
            { Type.Types.PrimitiveTypeId.Float, FloatHandler },
            { Type.Types.PrimitiveTypeId.Double, DoubleHandler },
            { Type.Types.PrimitiveTypeId.String, TextTypeHandler },
            { Type.Types.PrimitiveTypeId.Utf8, Utf8Handler },
            { Type.Types.PrimitiveTypeId.Uuid, GuidHandler },
            { Type.Types.PrimitiveTypeId.Yson, YsonHandler },
            { Type.Types.PrimitiveTypeId.Json, JsonHandler },
            { Type.Types.PrimitiveTypeId.Date, DateOnlyHandler },
            { Type.Types.PrimitiveTypeId.Datetime, DateTimeHandler },
            { Type.Types.PrimitiveTypeId.Timestamp, TimstampHandler },

            //TODO: Implement these types
            { Type.Types.PrimitiveTypeId.Interval, UnknownHandler },
            { Type.Types.PrimitiveTypeId.TzDate, UnknownHandler },
            { Type.Types.PrimitiveTypeId.TzDatetime, UnknownHandler },
            { Type.Types.PrimitiveTypeId.TzTimestamp, UnknownHandler }
        };

    private static readonly Dictionary<System.Type, YdbTypeHandler> ClrTypeToDataYdbTypeHandlers;

    private static readonly HashSet<System.Type> ValueTupleTypes = new(new[]
    {
        typeof(Tuple<>),
        typeof(Tuple<,>),
        typeof(Tuple<,,>),
        typeof(Tuple<,,,>),
        typeof(Tuple<,,,,>),
        typeof(Tuple<,,,,,>),
        typeof(Tuple<,,,,,,>),
        typeof(Tuple<,,,,,,,>)
    });

    private readonly DictionaryHandler _dictHandler = new();

    private readonly ListHandler _listHandler = new();
    private readonly StructHandler _structHandler = new();
    private readonly TupleHandler _tupleHandler = new();

    static BuiltInTypeHandlerResolver()
    {
        ClrTypeToDataYdbTypeHandlers = new Dictionary<System.Type, YdbTypeHandler>
        {
            { typeof(int), Int32Handler },
            { typeof(bool), BoolHandler },
            { typeof(byte), Int8Handler },
            { typeof(sbyte), UInt8Handler },
            { typeof(short), Int16Handler },
            { typeof(ushort), UInt16Handler },
            { typeof(uint), UInt32Handler },
            { typeof(long), Int64Handler },
            { typeof(ulong), Uint64Handler },
            { typeof(float), FloatHandler },
            { typeof(double), DoubleHandler },
            { typeof(string), TextTypeHandler },
            { typeof(Guid), GuidHandler },
            { typeof(DateOnly), DateOnlyHandler },
            { typeof(DateTime), DateTimeHandler },

            { typeof(Timestamp), TimstampHandler },
            { typeof(Utf8String), Utf8Handler },

            { typeof(JsonValue), JsonHandler }


            // { typeof(Timestamp), TimstanpHandler },
            // { typeof(Yson), YsonHandler },
            // { typeof(Json), JsonHandler },
            // { typeof(Utf8), Utf8Handler },
        };
    }

    public override YdbTypeHandler? ResolveByDataTypeName(string typeName)
    {
        return typeName switch
        {
            "Int32" => Int32Handler
        };
    }

    public override YdbTypeHandler? ResolveByYdbType(Type type)
    {
        return ResolveByYdbTypeInternal(type, 0);
    }

    private YdbTypeHandler? ResolveByYdbTypeInternal(Type type, int recursion)
    {
        if (recursion >= 256)
            throw new YdbDriverException($"Possible recursion during resolving handler for type `{type}`");

        return type.TypeCase switch
        {
            Type.TypeOneofCase.TypeId => ResolveByPrimitiveId(type.TypeId),
            Type.TypeOneofCase.OptionalType =>
                ResolveByYdbTypeInternal(type.OptionalType.Item, recursion + 1),
            Type.TypeOneofCase.DecimalType => DecimalHandler,

            // CONTAINERS NOT IMPLEMENTED DURING YDB RETURNS 'NULL' FOR THESE TYPES
            // EXAMPLE:
            //  $struct = <|a:1|>; SELECT $struct;
            // should return struct, but returns null. Need investigation

            Type.TypeOneofCase.ListType => _listHandler,
            Type.TypeOneofCase.DictType => _dictHandler,
            Type.TypeOneofCase.EmptyDictType => _dictHandler,
            Type.TypeOneofCase.TupleType => _tupleHandler,
            Type.TypeOneofCase.StructType => _structHandler,

            // global::Ydb.Type.TypeOneofCase.VariantType => expr,
            // global::Ydb.Type.TypeOneofCase.TaggedType => expr,
            // global::Ydb.Type.TypeOneofCase.VoidType => expr,
            // global::Ydb.Type.TypeOneofCase.NullType => expr,
            // global::Ydb.Type.TypeOneofCase.EmptyListType => expr,

            // global::Ydb.Type.TypeOneofCase.PgType => expr,

            Type.TypeOneofCase.None => null,

            _ => throw new ArgumentOutOfRangeException(nameof(type.TypeCase))
        };
    }

    private YdbTypeHandler? ResolveByPrimitiveId(Type.Types.PrimitiveTypeId typeTypeId)
    {
        return YdbTypeToHandlerTable.TryGetValue(typeTypeId, out var handler) ? handler : null;
    }

    public override YdbTypeHandler? ResolveByClrType(System.Type type)
    {
        if (type.IsArray) return _listHandler;

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(List<>))
                return _listHandler;
            if (genericTypeDefinition == typeof(Dictionary<,>))
                return _dictHandler;
            if (ValueTupleTypes.Contains(genericTypeDefinition)) return _tupleHandler;
        }

        return ClrTypeToDataYdbTypeHandlers.TryGetValue(type, out var handler) ? handler : null;
    }
}