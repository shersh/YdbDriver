using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public interface IUserTypeMapping
{
    public Type YdbType { get; }
    public System.Type ClrType { get; }

    public YdbTypeHandler CreateHandler();
}