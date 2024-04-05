namespace RestSharp.Tests.Integrated;

public class ProxyTests {
    [Fact]
    public async Task Set_Invalid_Proxy_Fails() {
        using var server = WireMockServer.Start();
        using var client = new RestClient(new RestClientOptions(server.Url!) { Proxy = new WebProxy("non_existent_proxy", false) });

        var request  = new RestRequest();
        var response = await client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.ErrorException.Should().BeOfType<HttpRequestException>();
    }
}