using System.Text.Json;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Integrated.Server;
using static RestSharp.Tests.Integrated.Server.HttpServer;

namespace RestSharp.Tests.Integrated; 

[Collection(nameof(TestServerCollection))]
public class PutTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;

    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public PutTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output  = output;
        _client  = new RestClient(fixture.Server.Url);
    }

    [Fact]
    public async Task Should_put_json_body() {
        var body     = new TestRequest("foo", 100);
        var request  = new RestRequest(ContentResource).AddJsonBody(body);

        var response = await _client.PutAsync(request);

        var expected = JsonSerializer.Serialize(body, Options);
        response!.Content.Should().Be(expected);
    }

    [Fact]
    public async Task Should_put_json_body_using_extension() {
        var body     = new TestRequest("foo", 100);
        var response = await _client.PutJsonAsync<TestRequest, TestRequest>(ContentResource, body);
        
        response.Should().BeEquivalentTo(body);
    }

    [Fact]
    public async Task Can_Timeout_PUT_Async() {
        var request = new RestRequest(TimeoutResource, Method.Put).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 200;

        var response = await _client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }
    
}

public record TestRequest(string Data, int Number);