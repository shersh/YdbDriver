using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public interface IContainerHandler
{
    void SetMapper(TypeMapper mapper);
    TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription);
    void WriteContainer<TAny>(TAny value, Value dest);
}