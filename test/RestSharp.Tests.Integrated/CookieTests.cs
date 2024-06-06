using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using RestSharp.Tests.Integrated.Fixtures;
using WireMock.Types;
using WireMock.Util;

namespace RestSharp.Tests.Integrated;

public sealed class CookieTests : IDisposable {
    readonly RestClient     _client;
    readonly string         _host;
    readonly WireMockServer _server = WireMockServer.Start();

    public CookieTests() {
        var options = new RestClientOptions(_server.Url!) {
            CookieContainer = new CookieContainer()
        };
        _client = new RestClient(options);
        _host   = _client.Options.BaseUrl!.Host;

        _server
            .Given(Request.Create().WithPath("/get-cookies"))
            .RespondWith(Response.Create().WithCallback(HandleGetCookies));

        _server
            .Given(Request.Create().WithPath("/set-cookies"))
            .RespondWith(Response.Create().WithCallback(HandleSetCookies));
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
    public async Task Can_Perform_GET_Async_With_Request_And_Client_Cookies() {
        _client.Options.CookieContainer!.Add(new Cookie("clientCookie", "clientCookieValue", null, _host));

        var request = new RestRequest("get-cookies") {
            CookieContainer = new CookieContainer()
        };
        request.CookieContainer.Add(new Cookie("cookie", "value", null, _host));
        request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _host));
        var response = await _client.ExecuteAsync<string[]>(request);

        var expected = new[] { "cookie=value", "cookie2=value2", "clientCookie=clientCookieValue" };
        response.Data.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Response_Cookies() {
        var request  = new RestRequest("set-cookies");
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("success");

        AssertCookie("cookie1", "value1", x => x == DateTime.MinValue);
        response.Cookies.Find("cookie2").Should().BeNull("Cookie 2 should vanish as the path will not match");
        AssertCookie("cookie3", "value3", x => x > DateTime.Now);
        AssertCookie("cookie4", "value4", x => x > DateTime.Now);
        response.Cookies.Find("cookie5").Should().BeNull("Cookie 5 should vanish as the request is not SSL");
        AssertCookie("cookie6", "value6", x => x == DateTime.MinValue, true);
        return;

        void AssertCookie(string name, string value, Func<DateTime, bool> checkExpiration, bool httpOnly = false) {
            var c = response.Cookies.Find(name)!;
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

        var notFoundCookie = response.Cookies.Find("cookie_empty_domain");
        notFoundCookie.Should().BeNull();

        var emptyDomainCookieHeader = response
            .GetHeaderValues(KnownHeaders.SetCookie)
            .SingleOrDefault(h => h.StartsWith("cookie_empty_domain"));
        emptyDomainCookieHeader.Should().NotBeNull();
        emptyDomainCookieHeader.Should().Contain("domain=;");
    }

    static ResponseMessage HandleGetCookies(IRequestMessage request) {
        var response = request.Cookies!.Select(x => $"{x.Key}={x.Value}").ToArray();
        return WireMockTestServer.CreateJson(response);
    }

    static ResponseMessage HandleSetCookies(IRequestMessage request) {
        var cookies = new List<CookieInternal> {
            new("cookie1", "value1", new CookieOptions()),
            new("cookie2", "value2", new CookieOptions { Path                             = "/path_extra" }),
            new("cookie3", "value3", new CookieOptions { Expires                          = DateTimeOffset.Now.AddDays(2) }),
            new("cookie4", "value4", new CookieOptions { MaxAge                           = TimeSpan.FromSeconds(100) }),
            new("cookie5", "value5", new CookieOptions { Secure                           = true }),
            new("cookie6", "value6", new CookieOptions { HttpOnly                         = true }),
            new("cookie_empty_domain", "value_empty_domain", new CookieOptions { HttpOnly = true, Domain = string.Empty })
        };

        var response = new ResponseMessage {
            Headers = new Dictionary<string, WireMockList<string>>(),
            BodyData = new BodyData {
                DetectedBodyType = BodyType.String,
                BodyAsString     = "success"
            }
        };

        var valuesList = new WireMockList<string>();
        valuesList.AddRange(cookies.Select(cookie => cookie.Options.GetHeader(cookie.Name, cookie.Value)));
        response.Headers.Add(KnownHeaders.SetCookie, valuesList);

        return response;
    }

    record CookieInternal(string Name, string Value, CookieOptions Options);

    public void Dispose() {
        _client.Dispose();
        _server.Dispose();
    }
}

static class CookieExtensions {
    public static string GetHeader(this CookieOptions self, string name, string value) {
        var cookieHeader = new SetCookieHeaderValue((StringSegment)name, (StringSegment)value) {
            Domain   = (StringSegment)self.Domain,
            Path     = (StringSegment)self.Path,
            Expires  = self.Expires,
            Secure   = self.Secure,
            HttpOnly = self.HttpOnly,
            MaxAge   = self.MaxAge,
            SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)self.SameSite
        };
        return cookieHeader.ToString();
    }
}