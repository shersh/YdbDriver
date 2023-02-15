using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public abstract class ContainerHandlerBase<T> : YdbTypeHandler<T>, IContainerHandler
{
    protected TypeMapper? Mapper;

    public void SetMapper(TypeMapper mapper)
    {
        Mapper = mapper;
    }

    public abstract TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription);
    public abstract void WriteContainer<TAny>(TAny value, Value dest);
}