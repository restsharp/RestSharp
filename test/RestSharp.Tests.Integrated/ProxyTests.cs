using System.Net;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated; 

public class ProxyTests {
    [Fact]
    public async Task Set_Invalid_Proxy_Fails() {
        using var server = HttpServerFixture.StartServer((_, __) => { });

        var client  = new RestClient(new RestClientOptions(server.Url) { Proxy = new WebProxy("non_existent_proxy", false) });
        var request = new RestRequest();

        var response = await client.ExecuteAsync(request);

        Assert.False(response.IsSuccessful);
        response.ErrorException.Should().BeOfType<HttpRequestException>();
    }
}