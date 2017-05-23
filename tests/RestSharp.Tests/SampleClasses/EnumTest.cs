
namespace RestSharp.Tests.SampleClasses
{
    public enum ByteEnum : byte
    {
        EnumMin = 0,
        EnumMax = 255
    }

    public enum SByteEnum : sbyte
    {
        EnumMin = -128,
        EnumMax = 127
    }

    public enum ShortEnum : short
    {
        EnumMin = -32768,
        EnumMax = 32767
    }

    public enum UShortEnum : ushort
    {
        EnumMin = 0,
        EnumMax = 65535
    }

    public enum IntEnum
    {
        EnumMin = -2147483648,
        EnumMax = 2147483647
    }

    public enum UIntEnum : uint
    {
        EnumMin = 0,
        EnumMax = 4294967295
    }

    public enum LongEnum : long
    {
        EnumMin = -9223372036854775808,
        EnumMax = 9223372036854775807
    }

    public enum ULongEnum : ulong
    {
        EnumMin = 0,
        EnumMax = 18446744073709551615
    }

    public class JsonEnumTypesTestStructure
    {
        public ByteEnum ByteEnumType { get; set; }

        public SByteEnum SByteEnumType { get; set; }

        public ShortEnum ShortEnumType { get; set; }

        public UShortEnum UShortEnumType { get; set; }

        public IntEnum IntEnumType { get; set; }

        public UIntEnum UIntEnumType { get; set; }

        public LongEnum LongEnumType { get; set; }

        public ULongEnum ULongEnumType { get; set; }
    }
}
