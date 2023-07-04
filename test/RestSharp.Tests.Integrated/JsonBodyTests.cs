using System.Text.Json;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

#pragma warning disable xUnit1033
public sealed class JsonBodyTests : IClassFixture<RequestBodyFixture> {
    readonly SimpleServer      _server;
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;

    public JsonBodyTests(RequestBodyFixture fixture, ITestOutputHelper output) {
        _output = output;
        _server = fixture.Server;
        _client = new RestClient(_server.Url);
    }

    [Fact]
    public async Task Query_Parameters_With_Json_Body() {
        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Put)
            .AddJsonBody(new { displayName = "Display Name" })
            .AddQueryParameter("key", "value");

        await _client.ExecuteAsync(request);

        RequestBodyCapturer.CapturedUrl.ToString().Should().Be($"{_server.Url}Capture?key=value");
        RequestBodyCapturer.CapturedContentType.Should().Be("application/json; charset=utf-8");
        RequestBodyCapturer.CapturedEntityBody.Should().Be("{\"displayName\":\"Display Name\"}");
    }

    [Fact]
    public async Task Add_JSON_body_JSON_string() {
        const string payload = "{\"displayName\":\"Display Name\"}";

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post).AddJsonBody(payload);

        await _client.ExecuteAsync(request);

        RequestBodyCapturer.CapturedContentType.Should().Be("application/json; charset=utf-8");
        RequestBodyCapturer.CapturedEntityBody.Should().Be(payload);
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
        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post).AddJsonBody(payload, true);

        await _client.ExecuteAsync(request);

        RequestBodyCapturer.CapturedContentType.Should().Be("application/json; charset=utf-8");
        RequestBodyCapturer.CapturedEntityBody.Should().Be(expected);
    }
}
