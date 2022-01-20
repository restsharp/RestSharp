using RestSharp.Serializers;

namespace RestSharp.Tests.Serializers.Xml.SampleClasses;

[DeserializeAs(Name = "oddballRootName")]
public class Oddball {
    public string Sid { get; set; }

    public string FriendlyName { get; set; }

    [DeserializeAs(Name = "oddballPropertyName")]
    public string GoodPropertyName { get; set; }

    [DeserializeAs(Name = "oddballListName")]
    public List<string> ListWithGoodName { get; set; }
}