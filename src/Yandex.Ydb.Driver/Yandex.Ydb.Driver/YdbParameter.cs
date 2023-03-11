using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;

namespace Yandex.Ydb.Driver;

public sealed class YdbParameter<T> : YdbParameter
{
    public YdbParameter(string name, T value)
    {
        TypedValue = value;
        ParameterName = name;
    }

    public T? TypedValue { get; set; }

    public override object? Value
    {
        get => TypedValue;
        set => TypedValue = (T?)value;
    }

    internal override void ResolveHandler(TypeMapper mapper)
    {
        _handler = mapper.ResolveByValue(TypedValue);
    }

    public override TypedValue ToProto()
    {
        var proto = new TypedValue
        {
            Value = new Value()
        };

        Debug.Assert(_handler != null, nameof(_handler) + " != null");

        _handler.Write(TypedValue, proto.Value);
        proto.Type = _handler.GetYdbType(TypedValue);

        return proto;
    }
}

public class YdbParameter : DbParameter
{
    protected YdbTypeHandler? _handler;
    private object? _value;

    public YdbParameter()
    {
        ParameterName = string.Empty;
        SourceColumn = string.Empty;
        ResetDbType();
    }

    public YdbParameter(string name, object value)
    {
        ResetDbType();
        ParameterName = name;
        Value = value;
    }

    public override DbType DbType { get; set; } = DbType.Object;
    public override ParameterDirection Direction { get; set; }

    public override bool IsNullable { get; set; }
    
    [AllowNull]
    public override string ParameterName { get; set; }
    
    [AllowNull]
    public override string SourceColumn { get; set; }

    public override object? Value
    {
        get => _value;
        set
        {
            _value = value;
            _handler = null;
        }
    }

    public override bool SourceColumnNullMapping { get; set; }

    public override int Size { get; set; }

    internal virtual void ResolveHandler(TypeMapper mapper)
    {
        _handler = mapper.ResolveByValue(_value);
    }

    public override void ResetDbType()
    {
        DbType = DbType.Object;
    }

    public virtual TypedValue ToProto()
    {
        var proto = new TypedValue
        {
            Value = new Value()
        };

        Debug.Assert(_handler != null, nameof(_handler) + " != null");

        _handler.Write(_value, proto.Value);
        proto.Type = _handler.GetYdbType(_value);

        return proto;
    }
}