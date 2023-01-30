using Yandex.Ydb.Driver.Internal.TypeMapping;

namespace Yandex.Ydb.Driver.Internal.TypeHandling;

internal sealed class BuiltInTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    public override TypeHandlerResolver Create()
    {
        return new BuiltInTypeHandlerResolver();
    }
}