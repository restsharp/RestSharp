using System.Text.Json;

namespace RestSharp.Tests.Integrated;

public sealed class PutTests(WireMockTestServer server) : IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(server.Url!);

    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Should_put_json_body() {
        var body    = new TestRequest("foo", 100);
        var request = new RestRequest("/content").AddJsonBody(body);

        var response = await _client.PutAsync(request);

        var expected = JsonSerializer.Serialize(body, Options);
        response.Content.Should().Be(expected);
    }

    [Fact]
    public async Task Should_put_json_body_using_extension() {
        var body     = new TestRequest("foo", 100);
        var response = await _client.PutJsonAsync<TestRequest, TestRequest>("/content", body);

        response.Should().BeEquivalentTo(body);
    }

    [Fact]
    public async Task Can_Timeout_PUT_Async() {
        var request = new RestRequest("/timeout", Method.Put).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = TimeSpan.FromMilliseconds(200);

        var response = await _client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    public void Dispose() => _client.Dispose();
}
