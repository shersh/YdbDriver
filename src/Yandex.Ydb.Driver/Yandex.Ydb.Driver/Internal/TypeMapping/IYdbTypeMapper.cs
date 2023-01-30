namespace Yandex.Ydb.Driver.Internal.TypeMapping;

public interface IYdbTypeMapper
{
    void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory);

    void Reset();
}