﻿using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class FloatHandler : YdbTypeHandler<float>, IYdbSimpleTypeHandler<float>
{
    public override float Read(Value value, FieldDescription? fieldDescription = null)
    {
        return value.FloatValue;
    }

    public override void Write(float value, Value dest)
    {
        dest.FloatValue = value;
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        return new()
        {
            TypeId = Type.Types.PrimitiveTypeId.Float
        };
    }
}