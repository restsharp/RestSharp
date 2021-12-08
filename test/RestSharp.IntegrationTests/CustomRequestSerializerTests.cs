using RestSharp.Serializers;
using RestSharp.Serializers.Xml;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class CustomRequestSerializerTests {
    [Fact]
    public void Should_use_custom_xml_serializer() {
        using var server = SimpleServer.Create();

        var client     = new RestClient(server.Url);
        var serializer = new CustomXmlSerializer();
        var body       = new { Text = "text" };

        var request = new RestRequest("/") { XmlSerializer = serializer };
        request.AddXmlBody(body);
        client.Execute(request);

        serializer.BodyString.Should().Be(body.ToString());
    }

    [Fact]
    public void Should_use_custom_json_serializer_for_addbody() {
        using var server = SimpleServer.Create();

        var client     = new RestClient(server.Url);
        var serializer = new CustomJsonSerializer();
        var body       = new { Text = "text" };

        var request = new RestRequest("/") { JsonSerializer = serializer, RequestFormat = DataFormat.Json };
        request.AddBody(body);
        client.Execute(request);

        serializer.BodyString.Should().Be(body.ToString());
    }

    [Fact]
    public void Should_use_custom_json_serializer() {
        using var server = SimpleServer.Create();

        var client     = new RestClient(server.Url);
        var serializer = new CustomJsonSerializer();
        var body       = new { Text = "text" };

        var request = new RestRequest("/") { JsonSerializer = serializer };
        request.AddJsonBody(body);
        client.Execute(request);

        serializer.BodyString.Should().Be(body.ToString());
    }

    class CustomXmlSerializer : IXmlSerializer {
        public string BodyString { get; private set; }

        public string Serialize(object obj) => BodyString = obj?.ToString();

        public string ContentType { get; set; } = Serializers.ContentType.Xml;
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }

    class CustomJsonSerializer : ISerializer {
        public string BodyString { get; private set; }

        public string Serialize(object obj) => BodyString = obj?.ToString();

        public string ContentType { get; set; } = Serializers.ContentType.Json;
    }
}