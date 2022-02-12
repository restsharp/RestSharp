using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class PostTests {
    readonly RestClient _client;

    public PostTests(TestServerFixture fixture) => _client = new RestClient(fixture.Server.Url);

    [Fact]
    public async Task Should_post_json() {
        var body     = new TestRequest("foo", 100);
        var request  = new RestRequest("post/json").AddJsonBody(body);
        var response = await _client.ExecutePostAsync<TestResponse>(request);

        response.Data.Message.Should().Be(body.Data);
    }
    
    [Fact]
    public async Task Should_post_json_with_PostAsync() {
        var body     = new TestRequest("foo", 100);
        var request  = new RestRequest("post/json").AddJsonBody(body);
        var response = await _client.PostAsync<TestResponse>(request);

        response.Message.Should().Be(body.Data);
    }
    
    [Fact]
    public async Task Should_post_json_with_PostJsonAsync() {
        var body     = new TestRequest("foo", 100);
        var response = await _client.PostJsonAsync<TestRequest, TestResponse>("post/json", body);

        response.Message.Should().Be(body.Data);
    }
}
