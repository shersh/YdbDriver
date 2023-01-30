using System.Text;
using System.Text.Json;

namespace Yandex.Ydb.Driver.Types.Primitives;

public class Timestamp
{
    public long Value { get; }

    public Timestamp(long unixts)
    {
        Value = unixts;
    }

    public static Timestamp GetCurrent() => new Timestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
}

public class JsonValue
{
    public string Value { get; }

    private JsonValue(string value)
    {
        Value = value;
    }

    public static JsonValue FromString(string str) => new JsonValue(str);

    public static JsonValue FromObject(object obj) => new JsonValue(JsonSerializer.Serialize(obj));

    public static JsonValue From<T>(T obj) => new JsonValue(JsonSerializer.Serialize(obj));

    public T? To<T>() => JsonSerializer.Deserialize<T>(Value);
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