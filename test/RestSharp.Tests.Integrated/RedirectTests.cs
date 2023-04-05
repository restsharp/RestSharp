using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class RedirectTests {
    readonly RestClient _client;
    readonly string     _host;

    public RedirectTests(TestServerFixture fixture, ITestOutputHelper output) {
        var options = new RestClientOptions(fixture.Server.Url) {
            FollowRedirects = false
        };
        _client = new RestClient(options);
        _host   = _client.Options.BaseUrl!.Host;
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Redirect() {
        const string val = "Works!";

        var request = new RestRequest("redirect");

        var response = await _client.ExecuteAsync<Response>(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be(val);
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Request_Cookies() {
        var request = new RestRequest("get-cookies-redirect") {
            CookieContainer = new CookieContainer(),
        };
        request.CookieContainer.Add(new Cookie("cookie", "value", null, _host));
        request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _host));
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("[\"cookie=value\",\"cookie2=value2\"]");
    }

    class Response {
        public string? Message { get; set; }
    }
}
