using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using RestSharp.Serialization.Xml;
using Shouldly;

namespace RestSharp.IntegrationTests
{
    public class CustomRequestSerializerTests
    {
        private SimpleServer _server;
        private const string BASE_URL = "http://localhost:8888/";
        
        [Test]
        public void Should_use_custom_xml_serializer()
        {
            using (SimpleServer.Create(BASE_URL))
            {
                var client = new RestClient(BASE_URL);
                var serializer = new CustomSerializer();
                var body = new {Text = "text"};
                
                var request = new RestRequest("/") {XmlSerializer = serializer};
                request.AddXmlBody(body);
                client.Execute(request);
                
                serializer.BodyString.ShouldBe(body.ToString());
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