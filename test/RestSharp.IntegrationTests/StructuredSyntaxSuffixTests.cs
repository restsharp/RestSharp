using System.Net;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class StructuredSyntaxSuffixTests : IDisposable {
    readonly TestHttpServer _server;
    readonly string         _url;

    class Person {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    const string XmlContent  = "<Person><name>Bob</name><age>50</age></Person>";
    const string JsonContent = @"{ ""name"":""Bob"", ""age"":50 }";

    public StructuredSyntaxSuffixTests() {
        _server = new TestHttpServer(0, "", HandleRequest);
        _url    = $"http://localhost:{_server.Port}";

        static void HandleRequest(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> p) {
            response.ContentType = request.QueryString["ct"];
            response.OutputStream.WriteStringUtf8(request.QueryString["c"]);
            response.StatusCode = 200;
        }
    }

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task By_default_application_json_content_type_should_deserialize_as_JSON() {
        var client = new RestClient(_url);

        var request = new RestRequest()
            .AddParameter("ct", "application/json")
            .AddParameter("c", JsonContent);

        var response = await client.ExecuteAsync<Person>(request);

        Assert.Equal("Bob", response.Data.Name);
        Assert.Equal(50, response.Data.Age);
    }

    [Fact]
    public async Task By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON() {
        var client = new RestClient(_url);

        var request = new RestRequest()
            .AddParameter("ct", "application/vnd.somebody.something+json")
            .AddParameter("c", JsonContent);

        var response = await client.ExecuteAsync<Person>(request);

        Assert.Equal("Bob", response.Data.Name);
        Assert.Equal(50, response.Data.Age);
    }

    [Fact]
    public async Task By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML() {
        var client = new RestClient(_url);

        var request = new RestRequest()
            .AddParameter("ct", "application/vnd.somebody.something+xml")
            .AddParameter("c", XmlContent);

        var response = await client.ExecuteAsync<Person>(request);

        Assert.Equal("Bob", response.Data.Name);
        Assert.Equal(50, response.Data.Age);
    }

    [Fact]
    public async Task By_default_text_xml_content_type_should_deserialize_as_XML() {
        var client = new RestClient(_url);

        var request = new RestRequest()
            .AddParameter("ct", "text/xml")
            .AddParameter("c", XmlContent);

        var response = await client.ExecuteAsync<Person>(request);

        Assert.Equal("Bob", response.Data.Name);
        Assert.Equal(50, response.Data.Age);
    }

    // [Fact]
    // public void Should_allow_wildcard_content_types_to_be_defined() {
    //     var client = new RestClient(_url);
    //
    //     // In spite of the content type, handle ALL structured syntax suffixes of "+xml" as JSON
    //     client.AddHandler("*+xml", new JsonSerializer());
    //
    //     var request = new RestRequest()
    //         .AddParameter("ct", "application/vnd.somebody.something+xml")
    //         .AddParameter("c", JsonContent);
    //
    //     var response = client.Execute<Person>(request);
    //
    //     Assert.Equal("Bob", response.Data.Name);
    //     Assert.Equal(50, response.Data.Age);
    // }
}