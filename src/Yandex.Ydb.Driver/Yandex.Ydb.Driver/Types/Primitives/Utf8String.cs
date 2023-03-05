using System.Text;
using System.Text.Json;

namespace Yandex.Ydb.Driver.Types.Primitives;

public class Timestamp
{
    public Timestamp(long unixts)
    {
        Value = unixts;
    }

    public long Value { get; }

    public static Timestamp GetCurrent()
    {
        return new(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}

public class JsonValue
{
    private JsonValue(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JsonValue FromString(string str)
    {
        return new(str);
    }

    public static JsonValue FromObject(object obj)
    {
        return new(JsonSerializer.Serialize(obj));
    }

    public static JsonValue From<T>(T obj)
    {
        return new(JsonSerializer.Serialize(obj));
    }

    public T? To<T>()
    {
        return JsonSerializer.Deserialize<T>(Value);
    }
}

public class Utf8String
{
    public Utf8String(string txt)
    {
        Value = Encoding.UTF8.GetBytes(txt);
    }

    public Utf8String(byte[] value)
    {
        Value = value;
    }

    public Utf8String(Span<byte> bytes)
    {
        Value = bytes.ToArray();
    }

    public byte[] Value { get; init; }
}