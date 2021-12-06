using System.Net;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class ProxyTests {
    [Fact(Skip = "Behaves strangely on Windows")]
    public void Set_Invalid_Proxy_Fails() {
        using var server = HttpServerFixture.StartServer((_, __) => { });

        var client  = new RestClient(server.Url) { Proxy = new WebProxy("non_existent_proxy", false) };
        var request = new RestRequest();

        var response = client.Get(request);

        Assert.False(response.IsSuccessful);
        response.ErrorException.Should().BeOfType<WebException>();
    }

    [Fact(Skip = "Behaves strangely on Windows")]
    public void Set_Invalid_Proxy_Fails_RAW() {
        using var server = HttpServerFixture.StartServer((_, __) => { });

        var requestUri = new Uri(new Uri(server.Url), "");
        var webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
        webRequest.Proxy = new WebProxy("non_existent_proxy", false);

        Assert.Throws<WebException>(() => webRequest.GetResponse());
    }
}