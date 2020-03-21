using RestSharp.Serialization.Xml;

namespace RestSharp.IntegrationTests.SampleDeserializers
{
    internal class CustomDeserializer : IXmlDeserializer
    {
        public bool UseBytes { get; } = false;

        public T Deserialize<T>(IRestResponse response) => default;

        public T Deserialize<T>(string payload)
        {
            throw new System.NotImplementedException();
        }

        public T DeserializeFromBytes<T>(byte[] payload)
        {
            throw new System.NotImplementedException();
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }
}