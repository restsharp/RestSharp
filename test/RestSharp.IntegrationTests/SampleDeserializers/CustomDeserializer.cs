using RestSharp.Serialization.Xml;

namespace RestSharp.IntegrationTests.SampleDeserializers; 

class CustomDeserializer : IXmlDeserializer {
    public T Deserialize<T>(IRestResponse response) => default;

    public string RootElement { get; set; }
    public string Namespace { get; set; }
    public string DateFormat { get; set; }
}