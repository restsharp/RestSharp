using System.Text;
using RestSharp.Serializers.Xml;
using WireMock.Types;
using WireMock.Util;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace RestSharp.Tests.Integrated;

public sealed class StructuredSyntaxSuffixTests : IDisposable {
    readonly WireMockServer _server;

    class Person {
        public string Name { get; set; } = null!;

        public int Age { get; set; }
    }

    const string XmlContent  = "<Person><name>Bob</name><age>50</age></Person>";
    const string JsonContent = """{ "name":"Bob", "age":50 }""";

    public StructuredSyntaxSuffixTests() {
        _server = WireMockServer.Start();
        _server.Given(Request.Create().WithPath("/").UsingGet()).RespondWith(Response.Create().WithCallback(Handle));
        return;

        static ResponseMessage Handle(IRequestMessage request) {
            var response = new ResponseMessage {
                Headers = new Dictionary<string, WireMockList<string>> {
                    [KnownHeaders.ContentType] = new(request.Query!["ct"])
                },
                StatusCode   = 200,
                BodyData = new BodyData {
                    BodyAsString = request.Query["c"].First(),
                    Encoding = Encoding.UTF8,
                    DetectedBodyType = BodyType.String
                }
            };
            return response;
        }
    }

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task By_default_application_json_content_type_should_deserialize_as_JSON() {
        using var client = new RestClient(_server.Url!);

        var request = new RestRequest()
            .AddParameter("ct", ContentType.Json)
            .AddParameter("c", JsonContent);

        var response = await client.ExecuteAsync<Person>(request);

        response.Data!.Name.Should().Be("Bob");
        response.Data.Age.Should().Be(50);
    }

    [Fact]
    public async Task By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON() {
        using var client = new RestClient(_server.Url!);

        var request = new RestRequest()
            .AddParameter("ct", "application/vnd.somebody.something+json")
            .AddParameter("c", JsonContent);

        var response = await client.ExecuteAsync<Person>(request);

        response.Data!.Name.Should().Be("Bob");
        response.Data.Age.Should().Be(50);
    }

    [Fact]
    public async Task By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML() {
        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseXmlSerializer());

        var request = new RestRequest()
            .AddParameter("ct", "application/vnd.somebody.something+xml")
            .AddParameter("c", XmlContent);

        var response = await client.ExecuteAsync<Person>(request);

        response.Data!.Name.Should().Be("Bob");
        response.Data.Age.Should().Be(50);
    }

    [Fact]
    public async Task By_default_text_xml_content_type_should_deserialize_as_XML() {
        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseXmlSerializer());

        var request = new RestRequest()
            .AddParameter("ct", "text/xml")
            .AddParameter("c", XmlContent);

        var response = await client.ExecuteAsync<Person>(request);

        Assert.Equal("Bob", response.Data!.Name);
        Assert.Equal(50, response.Data.Age);
    }
}