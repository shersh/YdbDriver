using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

internal sealed class GlobalTypeMapper : IYdbTypeMapper
{
    static GlobalTypeMapper()
    {
        Instance = new GlobalTypeMapper();
    }

    private GlobalTypeMapper()
    {
        Reset();
    }

    public static GlobalTypeMapper Instance { get; }

    internal ReaderWriterLockSlim Lock { get; }
        = new(LockRecursionPolicy.SupportsRecursion);

    internal List<TypeHandlerResolverFactory> ResolverFactories { get; } = new();

    public ConcurrentDictionary<string, IUserTypeMapping> UserTypeMappings { get; } = new();

    public void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory)
    {
        Lock.EnterWriteLock();
        try
        {
            var type = resolverFactory.GetType();
            if (ResolverFactories[0].GetType() == type)
            {
                ResolverFactories[0] = resolverFactory;
            }
            else
            {
                for (var i = 0; i < ResolverFactories.Count; i++)
                    if (ResolverFactories[i].GetType() == type)
                        ResolverFactories.RemoveAt(i);

                ResolverFactories.Insert(0, resolverFactory);
            }
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public void Reset()
    {
        Lock.EnterWriteLock();
        try
        {
            ResolverFactories.Clear();
            ResolverFactories.Add(new BuiltInTypeHandlerResolverFactory());
            UserTypeMappings.Clear();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    internal static string? DbTypeToYdbType(DbType ydbType)
    {
        return ydbType switch
        {
            DbType.SByte => "Int8",
            DbType.Int16 => "Int16",
            DbType.Int32 => "Int32",
            DbType.Int64 => "Int64",
            DbType.Byte => "Uint8",
            DbType.UInt16 => "Uint16",
            DbType.UInt32 => "Uint32",
            DbType.UInt64 => "Uint64",
            DbType.Double => "Double",
            DbType.Decimal => "Decimal",
            DbType.Single => "Float",
            DbType.String => "Text",
            DbType.Guid => "Uuid",
            DbType.DateTime => "Timestamp",
            DbType.DateTime2 => "Interval",
            DbType.DateTimeOffset => "TzTimestamp",
            DbType.Binary => "Byte",
            DbType.Boolean => "Bool",
            _ => "Text"
        };
    }

    internal static DbType YdbDbTypeToDbType(string ydbType)
    {
        return ydbType switch
        {
            // Numeric types
            "Int8" => DbType.SByte,
            "Int16" => DbType.Int16,
            "Int32" => DbType.Int32,
            "Int64" => DbType.Int64,

            "Uint8" => DbType.Byte,
            "Uint16" => DbType.UInt16,
            "Uint32" => DbType.UInt32,
            "Uint64" => DbType.UInt64,

            "Double" => DbType.Double,
            "Decimal" => DbType.Decimal,
            "Float" => DbType.Single,

            // Text types
            "String" => DbType.String,
            "Text" => DbType.String,
            "Utf8" => DbType.String,
            "Json" => DbType.String,
            "JsonDocument" => DbType.Binary,
            "Yson" => DbType.String,

            "Uuid" => DbType.Guid,

            // Date/time types
            "Timestamp" => DbType.DateTime,
            "Interval" => DbType.DateTime2,
            "TzDate" => DbType.DateTimeOffset,
            "TzDateTime" => DbType.DateTimeOffset,
            "TzTimestamp" => DbType.DateTimeOffset,

            // Misc data types
            "Byte" => DbType.Binary,
            "Bool" => DbType.Boolean,

            _ => DbType.Object
        };
    }
}