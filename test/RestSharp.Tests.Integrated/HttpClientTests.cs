using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

public class HttpClientTests : IDisposable {
    readonly WireMockServer _server = WireMockTestServer.StartTestServer();

    [Fact]
    public async Task ShouldUseBaseAddress() {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_server.Url!);
        using var client = new RestClient(httpClient);

        var request  = new RestRequest("success");
        var response = await client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    public void Dispose() => _server.Dispose();
}
