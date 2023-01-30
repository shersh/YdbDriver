using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class DecimalHandler : YdbTypeHandler<decimal>, IYdbSimpleTypeHandler<decimal>, IYdbTypeHandler<decimal>
{
    public override decimal Read(Value value, FieldDescription? fieldDescription = null)
    {
        var decimalType = fieldDescription!.Type.DecimalType;
        return decimal.Round(Convert.ToDecimal(value.Low128) / ((ulong)Math.Pow(10, decimalType.Scale)),
            (int)decimalType.Precision);
    }

    public override void Write(decimal value, Value dest)
    {
        throw new NotImplementedException();
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            DecimalType = new DecimalType()
            {
                //TODO: Add here values
            }
        };
}