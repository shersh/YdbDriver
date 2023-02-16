using System.Diagnostics;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

public sealed class StructHandler : ContainerHandlerBase<IDictionary<string, object?>>
{
    public override IDictionary<string, object?> Read(Value value, FieldDescription? fieldDescription = null)
    {
        Debug.Assert(fieldDescription != null, nameof(fieldDescription) + " != null");
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var members = fieldDescription.Type.StructType.Members;

        var dict = new Dictionary<string, object?>();
        for (var index = 0; index < members.Count; index++)
        {
            var member = members[index];
            var resolveByYdbType = Mapper.ResolveByYdbType(member.Type);

            dict[member.Name] = resolveByYdbType.ReadAsObject(value.Items[index],
                new FieldDescription(member.Type, member.Name, index, Mapper));
        }

        return dict;
    }

    public override void Write(IDictionary<string, object?> value, Value dest)
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        foreach (var (key, o) in value)
        {
            var val = new Value();
            var typeHandler = Mapper.ResolveByValue(o);
            typeHandler.Write(o, val);
            dest.Items.Add(val);
        }
    }

    protected override Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var structType = new StructType();
        var result = new Type() { StructType = structType };

        if (value is Dictionary<string, object?> dict)
        {
            foreach (var (key, o) in dict)
            {
                var typeHandler = Mapper.ResolveByValue(o);
                structType.Members.Add(new StructMember() { Name = key, Type = typeHandler.GetYdbType(o) });
            }
        }
        else
        {
            var handler = Mapper.ResolveByClrType(typeof(TDefault));
            return handler.GetYdbType(value);
        }

        return result;
    }

    public override TAny ReadContainerAs<TAny>(Value value, FieldDescription? fieldDescription)
    {
        if (typeof(TAny) == typeof(Dictionary<string, Object?>))
            return (TAny)Read(value, fieldDescription);

        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        var handler = Mapper.ResolveByClrType(typeof(TAny));
        return handler.Read<TAny>(value, fieldDescription);
    }

    public override void WriteContainer<TAny>(TAny value, Value dest)
    {
        Debug.Assert(Mapper != null, nameof(Mapper) + " != null");

        if (value is Dictionary<string, object?> dict)
        {
            Write(dict, dest);
        }
        else
        {
            var handler = Mapper.ResolveByClrType(typeof(TAny));
            handler.Write(value, dest);
        }
    }
}