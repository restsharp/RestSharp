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

    public void Dispose() => _client.Dispose();
}