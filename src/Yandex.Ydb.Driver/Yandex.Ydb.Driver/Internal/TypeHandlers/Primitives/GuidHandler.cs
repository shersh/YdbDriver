using System.Runtime.InteropServices;
using Ydb;
using Type = Ydb.Type;

namespace Yandex.Ydb.Driver.Internal.TypeHandlers.Primitives;

// public sealed class GuidHandler : YdbPrimitiveTypeHandler<Guid>
// {
//     public override void Write(byte value, Value dest) => throw new NotSupportedException();
//     public override void Write(bool value, Value dest) => throw new NotSupportedException();
//     public override void Write(int value, Value dest) => throw new NotSupportedException();
//     public override void Write(long value, Value dest) => throw new NotSupportedException();
//     public override void Write(sbyte value, Value dest) => throw new NotSupportedException();
//     public override void Write(short value, Value dest) => throw new NotSupportedException();
//     public override void Write(string value, Value dest) => throw new NotSupportedException();
//     public override void Write(uint value, Value dest) => throw new NotSupportedException();
//     public override void Write(ulong value, Value dest) => throw new NotSupportedException();
//     public override void Write(ushort value, Value dest) => throw new NotSupportedException();
//
//     public override object ReadAsObject(Value value, FieldDescription? fieldDescription = null) =>
//         throw new NotSupportedException();
// }

public sealed class GuidHandler : YdbTypeHandler<Guid>
{
    private static GuidConverter _converter;

    private static (long, long) FastGuidToLongs(Guid guid)
    {
        _converter.Guid = guid;
        return (_converter.Long1, _converter.Long2);
    }

    private static Guid FastLongsToGuid(long a, long b)
    {
        _converter.Long1 = a;
        _converter.Long2 = b;
        return _converter.Guid;
    }

    public override Guid Read(Value value, FieldDescription? fieldDescription = null)
    {
        var guid = FastLongsToGuid((long)value.Low128, (long)value.High128);
        return guid;
    }

    public override void Write(Guid value, Value dest)
    {
        var (low, high) = FastGuidToLongs(value);
        dest.Low128 = (ulong)low;
        dest.High128 = (ulong)high;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct GuidConverter
    {
        [FieldOffset(0)] public readonly decimal Decimal;
        [FieldOffset(0)] public Guid Guid;
        [FieldOffset(0)] public long Long1;
        [FieldOffset(8)] public long Long2;
    }

    protected override global::Ydb.Type GetYdbTypeInternal<TDefault>(TDefault? value) where TDefault : default =>
        new()
        {
            TypeId = global::Ydb.Type.Types.PrimitiveTypeId.Uuid
        };
}