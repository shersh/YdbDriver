using System.Buffers;
using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Yandex.Ydb.Driver.Internal.TypeHandlers;
using Yandex.Ydb.Driver.Internal.TypeMapping;
using Ydb;
using Ydb.Table;
using Type = System.Type;
using Value = Ydb.Value;

namespace Yandex.Ydb.Driver;

public sealed class YdbDbDataReader : DbDataReader
{
    private readonly RepeatedField<ResultSet?> _results;
    private readonly TypeMapper _typeMapper;

    private FieldDescription[]? _fields;
    private int _resultIndex = -1;
    private int _rowIndex = -1;

    public YdbDbDataReader(ExecuteQueryResult result, TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
        _results = result.ResultSets;
    }

    private ResultSet? CurrentTable => _resultIndex == -1 ? null : _results[_resultIndex];

    private Value? CurrentRow => CurrentTable?.Rows[_rowIndex];

    private RepeatedField<Value>? RowFields => CurrentRow?.Items;

    public override int FieldCount => CurrentTable?.Columns.Count ?? 0;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int RecordsAffected => CurrentTable.Rows.Count;
    public override bool HasRows => CurrentTable.Rows.Count > 0;

    public override bool IsClosed => true;

    /// <summary>
    ///     Gets a value indicating the depth of nesting for the current row.  Always returns zero.
    /// </summary>
    public override int Depth => 0;

    public override bool GetBoolean(int ordinal)
    {
        return GetFieldValue<bool>(ordinal);
    }

    public override byte GetByte(int ordinal)
    {
        return GetFieldValue<byte>(ordinal);
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        return GetFieldValue<char>(ordinal);
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        return GetField(ordinal).Type.TypeCase.ToString();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return GetFieldValue<DateTime>(ordinal);
    }

    public override decimal GetDecimal(int ordinal)
    {
        return GetFieldValue<decimal>(ordinal);
    }

    public override double GetDouble(int ordinal)
    {
        return GetFieldValue<double>(ordinal);
    }

    public override T GetFieldValue<T>(int ordinal)
    {
        var field = GetField(ordinal);
        return field.Handler.Read<T>(CurrentRow.Items[ordinal], field);
    }

    private FieldDescription GetField(int ordinal)
    {
        return _fields![ordinal];
    }

    public override Type GetFieldType(int ordinal)
    {
        return GetField(ordinal).FieldType;
    }

    public override float GetFloat(int ordinal)
    {
        return GetFieldValue<float>(ordinal);
    }

    public override Guid GetGuid(int ordinal)
    {
        return GetFieldValue<Guid>(ordinal);
    }

    public override short GetInt16(int ordinal)
    {
        return GetFieldValue<short>(ordinal);
    }

    public override int GetInt32(int ordinal)
    {
        return GetFieldValue<int>(ordinal);
    }

    public override long GetInt64(int ordinal)
    {
        return GetFieldValue<long>(ordinal);
    }

    public override string GetName(int ordinal)
    {
        return GetField(ordinal).Name;
    }

    public override int GetOrdinal(string name)
    {
        Debug.Assert(_fields != null, nameof(_fields) + " != null");

        for (var i = 0; i < _fields.Length; i++)
            if (_fields[i].Name == name)
                return i;

        throw new YdbDriverException($"No column with name `{name}`");
    }

    public override string GetString(int ordinal)
    {
        return GetFieldValue<string>(ordinal);
    }

    public override object GetValue(int ordinal)
    {
        var fieldDescription = GetField(ordinal);
        return fieldDescription.Handler.ReadAsObject(CurrentRow.Items[ordinal], fieldDescription);
    }

    public override int GetValues(object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var count = Math.Min(FieldCount, values.Length);
        for (var i = 0; i < count; i++)
            values[i] = GetValue(i);
        return count;
    }

    public override bool IsDBNull(int ordinal)
    {
        return RowFields[ordinal].NullFlagValue == NullValue.NullValue;
    }

    public override bool NextResult()
    {
        if (_fields is not null)
        {
            ArrayPool<FieldDescription>.Shared.Return(_fields);
            _fields = null;
        }

        _resultIndex++;

        if (_resultIndex >= _results.Count)
            return false;

        if (_results[_resultIndex].Columns.Count == 0)
            return false;

        _fields = ArrayPool<FieldDescription>.Shared.Rent(_results[_resultIndex].Columns.Count);
        for (var index = 0; index < _results[_resultIndex].Columns.Count; index++)
        {
            var column = _results[_resultIndex].Columns[index];
            var field = new FieldDescription(column.Type, column.Name, index, _typeMapper);
            field.ResolveHandler();

            _fields[index] = field;
        }

        _rowIndex = -1;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (_fields is not null) ArrayPool<FieldDescription>.Shared.Return(_fields);

        base.Dispose(disposing);
    }

    public override bool Read()
    {
        if (_resultIndex == -1 && !NextResult())
            return false;

        return ++_rowIndex < CurrentTable.Rows.Count;
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public sealed class FieldDescription
{
    private readonly TypeMapper _typeMapper;

    internal FieldDescription(global::Ydb.Type type, string name, int index, TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
        Type = type;
        Name = name;
        Index = index;
    }

    public global::Ydb.Type Type { get; }
    public string Name { get; }
    public int Index { get; }

    internal YdbTypeHandler Handler { get; private set; }

    internal Type FieldType => Handler.GetFieldType(this);

    internal void ResolveHandler()
    {
        Handler = _typeMapper.ResolveByYdbType(Type);
    }
}