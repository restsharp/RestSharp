using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class CookieTests {
    readonly RestClient _client;
    readonly string     _host;

    public CookieTests(TestServerFixture fixture) {
        _client = new RestClient(fixture.Server.Url);
        _host   = _client.Options.BaseUrl!.Host;
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Request_Cookies() {
        var request = new RestRequest("get-cookies") {
            CookieContainer = new CookieContainer()
        };
        request.CookieContainer.Add(new Cookie("cookie", "value", null, _host));
        request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _host));
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("[\"cookie=value\",\"cookie2=value2\"]");
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Response_Cookies() {
        var request  = new RestRequest("set-cookies");
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("success");

        AssertCookie("cookie1", "value1", x => x == DateTime.MinValue);
        FindCookie("cookie2").Should().BeNull("Cookie 2 should vanish as the path will not match");
        AssertCookie("cookie3", "value3", x => x > DateTime.Now);
        AssertCookie("cookie4", "value4", x => x > DateTime.Now);
        FindCookie("cookie5").Should().BeNull("Cookie 5 should vanish as the request is not SSL");
        AssertCookie("cookie6", "value6", x => x == DateTime.MinValue, true);

        Cookie? FindCookie(string name) =>response!.Cookies!.FirstOrDefault(p => p.Name == name);

        void AssertCookie(string name, string value, Func<DateTime, bool> checkExpiration, bool httpOnly = false) {
            var c = FindCookie(name)!;
            c.Value.Should().Be(value);
            c.Path.Should().Be("/");
            c.Domain.Should().Be(_host);
            checkExpiration(c.Expires).Should().BeTrue();
            c.HttpOnly.Should().Be(httpOnly);
        }
    }

    [Fact]
    public async Task GET_Async_With_Response_Cookies_Should_Not_Fail_With_Cookie_With_Empty_Domain() {
        var request  = new RestRequest("set-cookies");
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("success");

        Cookie? notFoundCookie = FindCookie("cookie_empty_domain");
        notFoundCookie.Should().BeNull();

        HeaderParameter? emptyDomainCookieHeader = response.Headers!
            .SingleOrDefault(h => h.Name == KnownHeaders.SetCookie && ((string)h.Value!).StartsWith("cookie_empty_domain"));
        emptyDomainCookieHeader.Should().NotBeNull();
        ((string)emptyDomainCookieHeader!.Value!).Should().Contain("domain=;");
        
        Cookie? FindCookie(string name) => response!.Cookies!.FirstOrDefault(p => p.Name == name);
    }
}
