namespace RestSharp.Tests.Integrated;

public sealed class HttpClientTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    [Fact]
    public async Task ShouldUseBaseAddress() {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(server.Url!);
        using var client = new RestClient(httpClient);

        var request  = new RestRequest("success");
        var response = await client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }
}