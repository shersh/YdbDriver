using System.Globalization;
using System.Text;
using Ydb;

namespace Yandex.Ydb.Driver.Helpers;

public static class YdbValueExt
{
    public static ReadOnlySpan<byte> GetSpan(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.BytesValue => value.BytesValue.Span,
            Value.ValueOneofCase.TextValue => Encoding.UTF8.GetBytes(value.TextValue),
            _ => default
        };
    }

    public static byte[] GetBytes(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.BytesValue => value.BytesValue.ToByteArray(),
            Value.ValueOneofCase.TextValue => Encoding.UTF8.GetBytes(value.TextValue),
            _ => default
        };
    }

    public static uint GetUInt32(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Uint32Value => value.Uint32Value,
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToUInt32(value.BoolValue),
            Value.ValueOneofCase.Int32Value => Convert.ToUInt32(value.Int32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToUInt32(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToUInt32(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToUInt32(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToUInt32(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToUInt32(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToUInt32(value.Low128),
            _ => default
        };
    }

    public static string? GetString(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.BytesValue => value.BytesValue.ToStringUtf8(),
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToString(value.BoolValue),
            Value.ValueOneofCase.Int32Value => Convert.ToString(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToString(value.Uint32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToString(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToString(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToString(value.FloatValue, CultureInfo.InvariantCulture),
            Value.ValueOneofCase.DoubleValue => Convert.ToString(value.DoubleValue, CultureInfo.InvariantCulture),
            Value.ValueOneofCase.TextValue => Convert.ToString(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToString(value.Low128),
            _ => default
        };
    }

    public static int GetInt32(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Int32Value => value.Int32Value,
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToInt32(value.BoolValue),
            Value.ValueOneofCase.Uint32Value => Convert.ToInt32(value.Uint32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToInt32(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToInt32(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToInt32(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToInt32(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToInt32(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToInt32(value.Low128),
            _ => default
        };
    }

    public static ulong GetUInt64(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Uint64Value => value.Uint64Value,
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToUInt64(value.BoolValue),
            Value.ValueOneofCase.Int32Value => Convert.ToUInt64(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToUInt64(value.Uint32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToUInt64(value.Int64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToUInt64(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToUInt64(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToUInt64(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToUInt64(value.Low128),
            _ => default
        };
    }

    public static long GetInt64(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Int64Value => value.Int64Value,
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToInt64(value.BoolValue),
            Value.ValueOneofCase.Int32Value => Convert.ToInt64(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToInt64(value.Uint32Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToInt64(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToInt64(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToInt64(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToInt64(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToInt64(value.Low128),
            _ => default
        };
    }

    public static ushort GetUInt16(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Int32Value => Convert.ToUInt16(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToUInt16(value.Uint32Value),
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToUInt16(value.BoolValue),
            Value.ValueOneofCase.Int64Value => Convert.ToUInt16(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToUInt16(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToUInt16(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToUInt16(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToUInt16(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToUInt16(value.Low128),
            _ => default
        };
    }

    public static short GetInt16(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Int32Value => Convert.ToInt16(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToInt16(value.Uint32Value),
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToInt16(value.BoolValue),
            Value.ValueOneofCase.Int64Value => Convert.ToInt16(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToInt16(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToInt16(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToInt16(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToInt16(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToInt16(value.Low128),
            _ => default
        };
    }

    public static sbyte GetSByte(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.Int32Value => Convert.ToSByte(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToSByte(value.Uint32Value),
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToSByte(value.BoolValue),
            Value.ValueOneofCase.Int64Value => Convert.ToSByte(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToSByte(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToSByte(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToSByte(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToSByte(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToSByte(value.Low128),
            _ => default
        };
    }

    public static byte GetByte(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => Convert.ToByte(value.BoolValue),
            Value.ValueOneofCase.Int32Value => Convert.ToByte(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToByte(value.Uint32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToByte(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToByte(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToByte(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToByte(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToByte(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToByte(value.Low128),
            _ => default
        };
    }

    public static bool GetBool(this Value value)
    {
        return value.ValueCase switch
        {
            Value.ValueOneofCase.None => default,
            Value.ValueOneofCase.BoolValue => value.BoolValue,
            Value.ValueOneofCase.Int32Value => Convert.ToBoolean(value.Int32Value),
            Value.ValueOneofCase.Uint32Value => Convert.ToBoolean(value.Uint32Value),
            Value.ValueOneofCase.Int64Value => Convert.ToBoolean(value.Int64Value),
            Value.ValueOneofCase.Uint64Value => Convert.ToBoolean(value.Uint64Value),
            Value.ValueOneofCase.FloatValue => Convert.ToBoolean(value.FloatValue),
            Value.ValueOneofCase.DoubleValue => Convert.ToBoolean(value.DoubleValue),
            Value.ValueOneofCase.TextValue => Convert.ToBoolean(value.TextValue),
            Value.ValueOneofCase.NullFlagValue => default,
            Value.ValueOneofCase.Low128 => Convert.ToBoolean(value.Low128),
            _ => default
        };
    }
}