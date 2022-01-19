using RestSharp.Serializers;

namespace RestSharp.Tests.Serializers.Xml.SampleClasses; 

[DeserializeAs(Name = "Color")]
public class ColorWithValue {
    public string Name { get; set; }
    public int Value { get; set; }
}