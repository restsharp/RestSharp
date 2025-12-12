using System.Globalization;

namespace RestSharp.Tests.Serializers.Csv;

class TestObject {
    public string   StringValue   { get; set; }
    public int      Int32Value    { get; set; }
    public decimal  DecimalValue  { get; set; }
    public double   DoubleValue   { get; set; }
    public float    SingleValue   { get; set; }
    public DateTime DateTimeValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }

    public const string Titles =
        $"{nameof(StringValue)},{nameof(Int32Value)},{nameof(DecimalValue)},{nameof(DoubleValue)},{nameof(SingleValue)},{nameof(DateTimeValue)},{nameof(TimeSpanValue)}";

    public string ToString(CultureInfo c) {
        var str = StringValue?.Contains(',') == true ? $"\"{StringValue}\"" : StringValue;
        return $"{str},{Int32Value},{DecimalValue.ToString(c)},{DoubleValue.ToString(c)},{SingleValue.ToString(c)},{DateTimeValue.ToString(c)},{TimeSpanValue.ToString()}";
    }
}