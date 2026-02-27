using System.Text.Json;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace RestSharp.Tests.Integrated;

/// <summary>
/// Tests for cookie behavior during redirects and custom redirect handling.
/// Verifies fixes for https://github.com/restsharp/RestSharp/issues/2077
/// and https://github.com/restsharp/RestSharp/issues/2059
/// </summary>
public sealed class CookieRedirectTests(WireMockTestServer server) : IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(server.Url!);

    RestClient CreateClient(Action<RestClientOptions>? configure = null) {
        var options = new RestClientOptions(server.Url!);
        configure?.Invoke(options);
        return new RestClient(options);
    }

    // ─── Cookie tests ────────────────────────────────────────────────────

    [Fact]
    public async Task Redirect_Should_Forward_Cookies_Set_During_Redirect() {
        using var client = CreateClient(o => o.CookieContainer = new());

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Contain("redirectCookie",
            "cookies from Set-Cookie headers on redirect responses should be forwarded to the final destination");
    }

    [Fact]
    public async Task Redirect_Should_Capture_SetCookie_From_Redirect_In_CookieContainer() {
        var cookieContainer = new CookieContainer();
        using var client = CreateClient(o => o.CookieContainer = cookieContainer);

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        cookieContainer.GetCookies(new Uri(server.Url!)).Cast<Cookie>()
            .Should().Contain(c => c.Name == "redirectCookie" && c.Value == "value1",
                "cookies from Set-Cookie headers on redirect responses should be stored in the CookieContainer");
    }

    [Fact]
    public async Task Redirect_With_Existing_Cookies_Should_Include_Both_Old_And_New_Cookies() {
        var host = new Uri(server.Url!).Host;
        using var client = CreateClient(o => o.CookieContainer = new());

        var request = new RestRequest("/set-cookie-and-redirect") {
            CookieContainer = new()
        };
        request.CookieContainer.Add(new Cookie("existingCookie", "existingValue", "/", host));

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Contain("existingCookie",
            "pre-existing cookies should be forwarded through redirects");
        response.Content.Should().Contain("redirectCookie",
            "cookies set during redirect should also arrive at the final destination");
    }

    // ─── FollowRedirects = false ─────────────────────────────────────────

    [Fact]
    public async Task FollowRedirects_False_Should_Return_Redirect_Response() {
        using var client = CreateClient(o => o.FollowRedirects = false);

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    // ─── Max redirects ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_Stop_After_MaxRedirects() {
        using var client = CreateClient(o => o.RedirectOptions = new RedirectOptions { MaxRedirects = 3 });

        var request  = new RestRequest("/redirect-countdown?n=10");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
    }

    [Fact]
    public async Task Should_Follow_All_Redirects_When_Under_MaxRedirects() {
        using var client = CreateClient(o => o.RedirectOptions = new RedirectOptions { MaxRedirects = 50 });

        var request  = new RestRequest("/redirect-countdown?n=5");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Contain("Done!");
    }

    // ─── Verb changes (parameterized) ────────────────────────────────────

    [Theory]
    [InlineData(302, "GET")]
    [InlineData(303, "GET")]
    [InlineData(307, "POST")]
    [InlineData(308, "POST")]
    public async Task Post_Redirect_Should_Use_Expected_Method(int statusCode, string expectedMethod) {
        using var client = CreateClient();

        var request = new RestRequest($"/redirect-with-status?status={statusCode}", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be(expectedMethod);
    }

    [Fact]
    public async Task Body_Should_Be_Dropped_When_Verb_Changes_To_Get() {
        using var client = CreateClient();

        var request = new RestRequest("/redirect-with-status?status=302", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("GET");
        doc.RootElement.GetProperty("Body").GetString().Should().BeEmpty();
    }

    // ─── Header forwarding ──────────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForwardHeaders_Controls_Custom_Header_Forwarding(bool forwardHeaders) {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions { ForwardHeaders = forwardHeaders }
        );

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("X-Custom-Header", "custom-value");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (forwardHeaders)
            response.Content.Should().Contain("X-Custom-Header").And.Contain("custom-value");
        else
            response.Content.Should().NotContain("X-Custom-Header");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForwardAuthorization_Controls_Auth_Header_Forwarding(bool forwardAuth) {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions { ForwardAuthorization = forwardAuth }
        );

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("Authorization", "Bearer test-token");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (forwardAuth)
            response.Content.Should().Contain("Bearer test-token");
        else
            response.Content.Should().NotContain("Bearer test-token");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task ForwardAuthorizationToExternalHost_Controls_Cross_Origin_Auth(
        bool allowExternal, bool expectAuth
    ) {
        // Create a second server (different port = different origin) with echo endpoint
        using var externalServer = WireMockServer.Start();
        externalServer
            .Given(Request.Create().WithPath("/echo-request"))
            .RespondWith(Response.Create().WithCallback(request => {
                var headers = request.Headers?
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Value))
                    ?? new Dictionary<string, string>();
                return WireMockTestServer.CreateJson(new { Method = request.Method, Headers = headers, Body = request.Body ?? "" });
            }));

        // Main server redirects to the external server
        var redirectPath = $"/redirect-external-{allowExternal}";
        server.Given(Request.Create().WithPath(redirectPath))
            .RespondWith(Response.Create().WithCallback(_ => new ResponseMessage {
                StatusCode = 302,
                Headers = new Dictionary<string, WireMockList<string>> {
                    ["Location"] = new(externalServer.Url + "/echo-request")
                }
            }));

        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions {
                ForwardAuthorization               = true,
                ForwardAuthorizationToExternalHost = allowExternal
            }
        );

        var request = new RestRequest(redirectPath);
        request.AddHeader("Authorization", "Bearer secret-token");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (expectAuth)
            response.Content.Should().Contain("Bearer secret-token");
        else
            response.Content.Should().NotContain("Bearer secret-token");
    }

    [Fact]
    public async Task ForwardCookies_False_Should_Not_Send_Cookies_On_Redirect() {
        var host = new Uri(server.Url!).Host;
        var cookieContainer = new CookieContainer();
        using var client = CreateClient(o => {
            o.CookieContainer = cookieContainer;
            o.RedirectOptions = new RedirectOptions { ForwardCookies = false };
        });

        var request = new RestRequest("/set-cookie-and-redirect?url=/echo-cookies") {
            CookieContainer = new()
        };
        request.CookieContainer.Add(new Cookie("existingCookie", "existingValue", "/", host));

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        cookieContainer.GetCookies(new Uri(server.Url!)).Cast<Cookie>().Should()
            .Contain(c => c.Name == "redirectCookie",
                "Set-Cookie should still be stored even when ForwardCookies is false");
        response.Content.Should().NotContain("existingCookie");
        response.Content.Should().NotContain("redirectCookie");
    }

    // ─── ForwardBody ────────────────────────────────────────────────────

    [Fact]
    public async Task ForwardBody_False_Should_Drop_Body_Even_When_Verb_Preserved() {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions { ForwardBody = false }
        );

        var request = new RestRequest("/redirect-with-status?status=307", Method.Post);
        request.AddJsonBody(new { data = "should-be-dropped" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("POST");
        doc.RootElement.GetProperty("Body").GetString().Should().BeEmpty();
    }

    [Fact]
    public async Task ForwardBody_True_Should_Forward_Body_When_Verb_Preserved() {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions { ForwardBody = true }
        );

        var request = new RestRequest("/redirect-with-status?status=307", Method.Post);
        request.AddJsonBody(new { data = "keep-me" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("POST");
        doc.RootElement.GetProperty("Body").GetString().Should().Contain("keep-me");
    }

    // ─── ForwardQuery ───────────────────────────────────────────────────

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task ForwardQuery_Controls_Query_String_Forwarding(bool forwardQuery, bool expectQuery) {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions { ForwardQuery = forwardQuery }
        );

        var request = new RestRequest("/redirect-no-query");
        request.AddQueryParameter("foo", "bar");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (expectQuery)
            response.ResponseUri!.Query.Should().Contain("foo=bar");
        else
            (response.ResponseUri!.Query ?? "").Should().NotContain("foo=bar");
    }

    // ─── RedirectStatusCodes customization ──────────────────────────────

    [Fact]
    public async Task Custom_RedirectStatusCodes_Should_Follow_Custom_Code() {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions {
                RedirectStatusCodes = [HttpStatusCode.Found, (HttpStatusCode)399]
            }
        );

        var request  = new RestRequest("/redirect-custom-status?status=399");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "399 should be treated as a redirect because it's in RedirectStatusCodes");
    }

    [Fact]
    public async Task Custom_RedirectStatusCodes_Should_Not_Follow_Excluded_Code() {
        using var client = CreateClient(o =>
            o.RedirectOptions = new RedirectOptions {
                RedirectStatusCodes = [(HttpStatusCode)399]
            }
        );

        var request  = new RestRequest("/redirect-with-status?status=302");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Found,
            "302 should NOT be followed because it's not in the custom RedirectStatusCodes");
    }

    // ─── FollowRedirectsToInsecure ──────────────────────────────────────

    [Theory]
    [InlineData(false, HttpStatusCode.Redirect)]
    [InlineData(true, HttpStatusCode.OK)]
    public async Task FollowRedirectsToInsecure_Controls_Https_To_Http_Redirect(
        bool allowInsecure, HttpStatusCode expectedStatus
    ) {
        using var httpsServer = WireMockServer.Start(new WireMock.Settings.WireMockServerSettings {
            Port   = 0,
            UseSSL = true
        });

        httpsServer
            .Given(Request.Create().WithPath("/https-redirect"))
            .RespondWith(Response.Create().WithCallback(_ => new ResponseMessage {
                StatusCode = 302,
                Headers = new Dictionary<string, WireMockList<string>> {
                    ["Location"] = new(server.Url + "/echo-request")
                }
            }));

        using var client = new RestClient(new RestClientOptions(httpsServer.Url!) {
            // Cert validation disabled intentionally: local test HTTPS server uses self-signed cert
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
            RedirectOptions = new RedirectOptions { FollowRedirectsToInsecure = allowInsecure }
        });

        var request  = new RestRequest("/https-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(expectedStatus);
    }

    public void Dispose() => _client.Dispose();
}
