using Ydb;

namespace Yandex.Ydb.Driver.Internal.TypeHandling;

public interface IYdbTypeHandler<TDefault>
{
    TDefault Read(Value value, FieldDescription? fieldDescription);

    void Write(TDefault value, Value dest);
}