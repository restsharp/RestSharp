using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers;
using Shouldly;

namespace RestSharp.IntegrationTests
{
    public class CustomRequestSerializerTests
    {
        private const string BASE_URL = "http://localhost:8888/";
        
        [Test]
        public void Should_use_custom_xml_serializer()
        {
            using (SimpleServer.Create(BASE_URL))
            {
                var client = new RestClient(BASE_URL);
                var serializer = new CustomXmlSerializer();
                var body = new {Text = "text"};
                
                var request = new RestRequest("/") {XmlSerializer = serializer};
                request.AddXmlBody(body);
                client.Execute(request);
                
                serializer.BodyString.ShouldBe(body.ToString());
            }
        }

        [Test]
        public void Should_use_custom_json_serializer_for_addbody()
        {
            using (SimpleServer.Create(BASE_URL))
            {
                var client = new RestClient(BASE_URL);
                var serializer = new CustomJsonSerializer();
                var body = new {Text = "text"};
                
                var request = new RestRequest("/") {JsonSerializer = serializer, RequestFormat = DataFormat.Json};
                request.AddBody(body);
                client.Execute(request);
                
                serializer.BodyString.ShouldBe(body.ToString());
            }
        }

        [Test]
        public void Should_use_custom_json_serializer()
        {
            using (SimpleServer.Create(BASE_URL))
            {
                var client = new RestClient(BASE_URL);
                var serializer = new CustomJsonSerializer();
                var body = new {Text = "text"};

                var request = new RestRequest("/") {JsonSerializer = serializer};
                request.AddJsonBody(body);
                client.Execute(request);

                serializer.BodyString.ShouldBe(body.ToString());
            }
        }

        private class CustomXmlSerializer : IXmlSerializer
        {
            public string BodyString { get; private set; }

            public string Serialize(object obj) => BodyString = obj?.ToString();

            public string ContentType { get; set; } = Serialization.ContentType.Xml;
            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
        }

        private class CustomJsonSerializer : ISerializer
        {
            public string BodyString { get; private set; }

            public string Serialize(object obj) => BodyString = obj?.ToString();

            public string ContentType { get; set; } = Serialization.ContentType.Json;
        }
    }
}