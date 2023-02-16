using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class TupleHandler : ContainerHandlerBase<ITuple>
{
    private System.Type GetGenericTupleType(FieldDescription tupleInfo)
    {
        return tupleInfo.Type.TupleType.Elements.Count switch
        {
            1 => typeof(Tuple<>),
            2 => typeof(Tuple<,>),
            3 => typeof(Tuple<,,>),
            4 => typeof(Tuple<,,,>),
            5 => typeof(Tuple<,,,,>),
            6 => typeof(Tuple<,,,,,>),
            7 => typeof(Tuple<,,,,,,>),
            _ => throw new NotSupportedException(
                $"Tuple with `{tupleInfo.Type.TupleType.Elements.Count}` count of elements is not supported. Use struct instead")
        };
    }

    public override ITuple Read(Value value, FieldDescription? fieldDescription = null)
    {
        Debug.Assert(fieldDescription != null, nameof(fieldDescription) + " != null");
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var tupleType = GetGenericTupleType(fieldDescription!);
        var tupleKeyTypes =
            fieldDescription.Type.TupleType.Elements.Select(x => new FieldDescription(x, string.Empty, 0, Mapper))
                .ToArray();

        var tupleKeyHandlers = tupleKeyTypes.Select(x => Mapper.ResolveByYdbType(x.Type)).ToArray();

        var tupleValues = new object[tupleKeyTypes.Length];

        for (var index = 0; index < value.Items.Count; index++)
        {
            var item = value.Items[index];
            tupleValues[index] = tupleKeyHandlers[index].ReadAsObject(item, tupleKeyTypes[index])!;
        }

        var tuple = Activator.CreateInstance(tupleType, tupleValues);
        return (ITuple)tuple!;
    }

    public override void Write(ITuple value, Value dest)
    {
        throw new NotImplementedException();
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        var tupleType = new TupleType();
        var arguments = typeof(TDefault).GetGenericArguments();
        var elements = arguments.Select(x => Mapper.ResolveByClrType(x).GetYdbType(value));
        tupleType.Elements.Add(elements);

        return new Type()
        {
            TupleType = tupleType
        };
    }

    public override TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription)
    {
        Debug.Assert(fieldDescription != null, nameof(fieldDescription) + " != null");
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var tupleKeyTypes =
            fieldDescription.Type.TupleType.Elements.Select(x => new FieldDescription(x, string.Empty, 0, Mapper))
                .ToArray();

        var tupleKeyHandlers = tupleKeyTypes.Select(x => Mapper.ResolveByYdbType(x.Type)).ToArray();
        var tupleValues = new object[tupleKeyTypes.Length];

        for (var index = 0; index < value.Items.Count; index++)
        {
            var item = value.Items[index];
            tupleValues[index] = tupleKeyHandlers[index].ReadAsObject(item, tupleKeyTypes[index])!;
        }

        var tuple = Activator.CreateInstance(typeof(TAny), tupleValues);
        return (TAny)tuple!;
    }

    public override void WriteContainer<TAny>(TAny value, Value dest)
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");
        var tupleType = typeof(TAny);

        if (value is not ITuple tuple) throw new NotSupportedException($"{tupleType.Name} is not supported");

        var tupleValueTypes = tupleType.GetGenericArguments();
        var tupleValueHandlers = tupleValueTypes.Select(x => Mapper.ResolveByClrType(x)).ToArray();

        for (int i = 0; i < tuple.Length; i++)
        {
            var val = new Value();
            tupleValueHandlers[i].Write(tuple[i], val);
            dest.Items.Add(val);
        }
    }
}