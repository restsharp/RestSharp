using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using RestSharp.Serialization.Xml;

namespace RestSharp.IntegrationTests
{
    public class CustomRequestSerializerTests
    {
        private SimpleServer _server;
        private const string BASE_URL = "http://localhost:8888/";
        
        [Test]
        public void Should_use_custom_xml_serializer()
        {
            using (var server = SimpleServer.Create(BASE_URL))
            {
                var client = new RestClient(BASE_URL);
                var request = new RestRequest("/") {XmlSerializer = new CustomSerializer()};
                request.AddXmlBody(new {Text = "text"});
                var result = client.Execute(request);
            }
        }

        private class CustomSerializer : IXmlSerializer
        {
            public string BodyString { get; private set; }

            public string Serialize(object obj) => BodyString = obj?.ToString();

            public string ContentType { get; set; } = "application/xml";
            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
        }
    }
}