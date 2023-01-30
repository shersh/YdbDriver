using Yandex.Ydb.Driver.Internal.TypeHandlers;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

internal interface IUserTypeMapping
{
    public string YdbTypeName { get; }
    public Type ClrType { get; }

    public YdbTypeHandler CreateHandler();
}