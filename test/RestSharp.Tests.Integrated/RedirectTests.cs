namespace RestSharp.Tests.Integrated;

public sealed class RedirectTests(WireMockTestServer server) : IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(new RestClientOptions(server.Url!) { FollowRedirects = true });

    [Fact]
    public async Task Can_Perform_GET_Async_With_Redirect() {
        const string val = "Works!";

        var request  = new RestRequest("redirect");
        var response = await _client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be(val);
    }

    [Fact]
    public async Task ResponseUri_Should_Be_Final_Url_When_FollowRedirects_True() {
        var request  = new RestRequest("redirect");
        var response = await _client.ExecuteAsync(request);

        response.ResponseUri.Should().Be(new Uri(new Uri(server.Url!), "success"));
    }

    [Fact]
    public async Task ResponseUri_Should_Be_Redirect_Target_When_FollowRedirects_False() {
        using var client = new RestClient(new RestClientOptions(server.Url!) { FollowRedirects = false });

        var request  = new RestRequest("redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.ResponseUri.Should().Be(new Uri(new Uri(server.Url!), "success"));
    }

    public void Dispose() => _client.Dispose();
}