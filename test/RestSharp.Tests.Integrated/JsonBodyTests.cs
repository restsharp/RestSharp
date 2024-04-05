using System.Text.Json;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

#pragma warning disable xUnit1033
public sealed class JsonBodyTests : IDisposable {
    readonly WireMockServer _server = WireMockServer.Start();
    readonly RestClient     _client;

    public JsonBodyTests() => _client = new RestClient(_server.Url!);

    [Fact]
    public async Task Query_Parameters_With_Json_Body() {
        var capturer = _server.ConfigureBodyCapturer(Method.Put);

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Put)
            .AddJsonBody(new { displayName = "Display Name" })
            .AddQueryParameter("key", "value");

        await _client.ExecuteAsync(request);

        capturer.ContentType.Should().Be("application/json; charset=utf-8");
        capturer.Body.Should().Be("{\"displayName\":\"Display Name\"}");
        capturer.Url.Should().Be($"{_server.Url}{RequestBodyCapturer.Resource}?key=value");
    }

    [Fact]
    public async Task Add_JSON_body_JSON_string() {
        const string payload = "{\"displayName\":\"Display Name\"}";

        var capturer = _server.ConfigureBodyCapturer(Method.Post);
        var request  = new RestRequest(RequestBodyCapturer.Resource, Method.Post).AddJsonBody(payload);

        await _client.ExecuteAsync(request);

        capturer.ContentType.Should().Be("application/json; charset=utf-8");
        capturer.Body.Should().Be(payload);
    }

    [Fact]
    public async Task Add_JSON_body_string() {
        const string payload = @"
""requestBody"": { 
    ""content"": { 
        ""application/json"": { 
            ""schema"": { 
                ""type"": ""string"" 
            } 
        } 
    } 
},";

        var expected = JsonSerializer.Serialize(payload);
        var capturer = _server.ConfigureBodyCapturer(Method.Post);
        var request  = new RestRequest(RequestBodyCapturer.Resource, Method.Post).AddJsonBody(payload, true);

        await _client.ExecuteAsync(request);

        capturer.ContentType.Should().Be("application/json; charset=utf-8");
        capturer.Body.Should().Be(expected);
    }

    public void Dispose() {
        _server.Dispose();
        _client.Dispose();
    }
}