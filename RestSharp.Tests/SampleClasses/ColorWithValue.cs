namespace RestSharp.Tests.SampleClasses
{
    [Deserializers.DeserializeAs(Name = "Color")]
    public class ColorWithValue
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}