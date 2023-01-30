using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class DoubleHandler : YdbTypeHandler<double>, IYdbSimpleTypeHandler<double>
{
    public override double Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.DoubleValue;
    }

    public override void Write(double value, Value dest)
    {
        dest.DoubleValue = value;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Double
        };
}