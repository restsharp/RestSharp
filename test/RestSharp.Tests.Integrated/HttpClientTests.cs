using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class HttpClientTests(TestServerFixture fixture) {
    [Fact]
    public async Task ShouldUseBaseAddress() {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = fixture.Server.Url;
        using var client = new RestClient(httpClient);

        var request  = new RestRequest("success");
        var response = await client.ExecuteAsync<Response>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    record Response(string Message);
}
