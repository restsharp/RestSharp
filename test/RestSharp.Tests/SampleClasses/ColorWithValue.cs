using RestSharp.Deserializers;

namespace RestSharp.Tests.SampleClasses
{
    [DeserializeAs(Name = "Color")]
    public class ColorWithValue
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}