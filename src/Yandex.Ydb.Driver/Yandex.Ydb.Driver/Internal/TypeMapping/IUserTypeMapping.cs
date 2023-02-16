using Yandex.Ydb.Driver.Internal.TypeHandlers;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public interface IUserTypeMapping
{
    public global::Ydb.Type YdbType { get; }
    public Type ClrType { get; }

    public YdbTypeHandler CreateHandler();
}