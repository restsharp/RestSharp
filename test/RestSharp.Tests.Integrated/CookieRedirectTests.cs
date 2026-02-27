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
public sealed class CookieRedirectTests : IDisposable {
    readonly WireMockServer _server = WireMockServer.Start();

    public CookieRedirectTests() {
        // Endpoint that sets a cookie via Set-Cookie header and redirects to /echo-cookies
        _server
            .Given(Request.Create().WithPath("/set-cookie-and-redirect").UsingGet())
            .RespondWith(Response.Create().WithCallback(request => {
                var url = "/echo-cookies";
                if (request.Query != null && request.Query.TryGetValue("url", out var urlValues)) {
                    url = urlValues[0];
                }

                var response = new ResponseMessage {
                    StatusCode = 302,
                    Headers    = new Dictionary<string, WireMockList<string>>()
                };
                response.Headers.Add("Location", new WireMockList<string>(url));
                response.Headers.Add(
                    "Set-Cookie",
                    new WireMockList<string>("redirectCookie=value1; Path=/")
                );
                return response;
            }));

        // Endpoint that echoes back received Cookie header values as JSON
        _server
            .Given(Request.Create().WithPath("/echo-cookies").UsingGet())
            .RespondWith(Response.Create().WithCallback(request => {
                var cookieHeaders = new List<string>();
                if (request.Headers != null && request.Headers.TryGetValue("Cookie", out var values)) {
                    cookieHeaders.AddRange(values);
                }

                var parsedCookies = request.Cookies?.Select(x => $"{x.Key}={x.Value}").ToList()
                    ?? new List<string>();

                return WireMockTestServer.CreateJson(new {
                    RawCookieHeaders = cookieHeaders,
                    ParsedCookies    = parsedCookies
                });
            }));

        // Endpoint that echoes request details (method, headers, body)
        _server
            .Given(Request.Create().WithPath("/echo-request"))
            .RespondWith(Response.Create().WithCallback(request => {
                var headers = request.Headers?
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Value))
                    ?? new Dictionary<string, string>();

                return WireMockTestServer.CreateJson(new {
                    Method  = request.Method,
                    Headers = headers,
                    Body    = request.Body ?? ""
                });
            }));

        // Redirect with configurable status code
        _server
            .Given(Request.Create().WithPath("/redirect-with-status"))
            .RespondWith(Response.Create().WithCallback(request => {
                var status = 302;
                var url    = "/echo-request";

                if (request.Query != null) {
                    if (request.Query.TryGetValue("status", out var statusValues))
                        status = int.Parse(statusValues[0]);
                    if (request.Query.TryGetValue("url", out var urlValues))
                        url = urlValues[0];
                }

                return new ResponseMessage {
                    StatusCode = status,
                    Headers = new Dictionary<string, WireMockList<string>> {
                        ["Location"] = new(url)
                    }
                };
            }));

        // Countdown redirect: 307 redirects to self with n-1, returns 200 when n<=1
        _server
            .Given(Request.Create().WithPath("/redirect-countdown"))
            .RespondWith(Response.Create().WithCallback(request => {
                var n = 1;
                if (request.Query != null && request.Query.TryGetValue("n", out var nValues))
                    n = int.Parse(nValues[0]);

                if (n <= 1) {
                    return WireMockTestServer.CreateJson(new { Message = "Done!" });
                }

                return new ResponseMessage {
                    StatusCode = (int)HttpStatusCode.TemporaryRedirect,
                    Headers = new Dictionary<string, WireMockList<string>> {
                        ["Location"] = new($"/redirect-countdown?n={n - 1}")
                    }
                };
            }));

        // Redirect to a path with no query (used for ForwardQuery tests)
        _server
            .Given(Request.Create().WithPath("/redirect-no-query"))
            .RespondWith(Response.Create().WithCallback(_ => new ResponseMessage {
                StatusCode = 302,
                Headers = new Dictionary<string, WireMockList<string>> {
                    ["Location"] = new("/echo-request")
                }
            }));

        // Echo query string parameters
        _server
            .Given(Request.Create().WithPath("/echo-query"))
            .RespondWith(Response.Create().WithCallback(request => {
                var query = request.Query?
                    .ToDictionary(x => x.Key, x => string.Join(",", x.Value))
                    ?? new Dictionary<string, string>();
                return WireMockTestServer.CreateJson(new { Query = query });
            }));

        // Redirect that preserves a custom status code for testing RedirectStatusCodes
        _server
            .Given(Request.Create().WithPath("/redirect-custom-status"))
            .RespondWith(Response.Create().WithCallback(request => {
                var status = 399;
                if (request.Query != null && request.Query.TryGetValue("status", out var statusValues))
                    status = int.Parse(statusValues[0]);

                return new ResponseMessage {
                    StatusCode = status,
                    Headers = new Dictionary<string, WireMockList<string>> {
                        ["Location"] = new("/echo-request")
                    }
                };
            }));
    }

    // ─── Cookie tests ────────────────────────────────────────────────────

    [Fact]
    public async Task Redirect_Should_Forward_Cookies_Set_During_Redirect() {
        var options = new RestClientOptions(_server.Url!) {
            FollowRedirects = true,
            CookieContainer = new()
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = response.Content!;

        content.Should().Contain("redirectCookie",
            "cookies from Set-Cookie headers on redirect responses should be forwarded to the final destination");
    }

    [Fact]
    public async Task Redirect_Should_Capture_SetCookie_From_Redirect_In_CookieContainer() {
        var options = new RestClientOptions(_server.Url!) {
            FollowRedirects = true,
            CookieContainer = new()
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = options.CookieContainer.GetCookies(new Uri(_server.Url!));
        cookies.Cast<Cookie>().Should().Contain(c => c.Name == "redirectCookie" && c.Value == "value1",
            "cookies from Set-Cookie headers on redirect responses should be stored in the CookieContainer");
    }

    [Fact]
    public async Task Redirect_With_Existing_Cookies_Should_Include_Both_Old_And_New_Cookies() {
        var host = new Uri(_server.Url!).Host;
        var options = new RestClientOptions(_server.Url!) {
            FollowRedirects = true,
            CookieContainer = new()
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/set-cookie-and-redirect") {
            CookieContainer = new()
        };
        request.CookieContainer.Add(new Cookie("existingCookie", "existingValue", "/", host));

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = response.Content!;

        content.Should().Contain("existingCookie",
            "pre-existing cookies should be forwarded through redirects");
        content.Should().Contain("redirectCookie",
            "cookies set during redirect should also arrive at the final destination");
    }

    // ─── FollowRedirects = false ─────────────────────────────────────────

    [Fact]
    public async Task FollowRedirects_False_Should_Return_Redirect_Response() {
        var options = new RestClientOptions(_server.Url!) {
            FollowRedirects = false
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/set-cookie-and-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    // ─── Max redirects ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_Stop_After_MaxRedirects() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { MaxRedirects = 3 }
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/redirect-countdown?n=10");
        var response = await client.ExecuteAsync(request);

        // After 3 redirects, should return the 4th redirect response as-is
        response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
    }

    [Fact]
    public async Task Should_Follow_All_Redirects_When_Under_MaxRedirects() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { MaxRedirects = 50 }
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/redirect-countdown?n=5");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Contain("Done!");
    }

    // ─── Verb changes ────────────────────────────────────────────────────

    [Fact]
    public async Task Post_Should_Become_Get_On_302() {
        var options = new RestClientOptions(_server.Url!);
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("GET");
    }

    [Fact]
    public async Task Post_Should_Become_Get_On_303() {
        var options = new RestClientOptions(_server.Url!);
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=303", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("GET");
    }

    [Fact]
    public async Task Post_Should_Stay_Post_On_307() {
        var options = new RestClientOptions(_server.Url!);
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=307", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("POST");
    }

    [Fact]
    public async Task Post_Should_Stay_Post_On_308() {
        var options = new RestClientOptions(_server.Url!);
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=308", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("POST");
    }

    [Fact]
    public async Task Body_Should_Be_Dropped_When_Verb_Changes_To_Get() {
        var options = new RestClientOptions(_server.Url!);
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302", Method.Post);
        request.AddJsonBody(new { data = "test" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("GET");
        doc.RootElement.GetProperty("Body").GetString().Should().BeEmpty();
    }

    // ─── Header forwarding ──────────────────────────────────────────────

    [Fact]
    public async Task ForwardHeaders_False_Should_Strip_Custom_Headers() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardHeaders = false }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("X-Custom-Header", "custom-value");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotContain("X-Custom-Header");
    }

    [Fact]
    public async Task ForwardAuthorization_False_Should_Strip_Authorization_Header() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardAuthorization = false }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("Authorization", "Bearer test-token");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotContain("Bearer test-token");
    }

    [Fact]
    public async Task ForwardCookies_False_Should_Not_Send_Cookies_On_Redirect() {
        var host = new Uri(_server.Url!).Host;
        var options = new RestClientOptions(_server.Url!) {
            CookieContainer = new(),
            RedirectOptions = new RedirectOptions { ForwardCookies = false }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/set-cookie-and-redirect?url=/echo-cookies") {
            CookieContainer = new()
        };
        request.CookieContainer.Add(new Cookie("existingCookie", "existingValue", "/", host));

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Cookies should still be stored in the container
        options.CookieContainer.GetCookies(new Uri(_server.Url!)).Cast<Cookie>().Should()
            .Contain(c => c.Name == "redirectCookie",
                "Set-Cookie should still be stored even when ForwardCookies is false");
        // But not forwarded to the redirect target
        response.Content.Should().NotContain("existingCookie",
            "cookies should not be sent to the redirect target when ForwardCookies is false");
        response.Content.Should().NotContain("redirectCookie",
            "cookies should not be sent to the redirect target when ForwardCookies is false");
    }

    // ─── ForwardHeaders = true (positive test) ─────────────────────────

    [Fact]
    public async Task ForwardHeaders_True_Should_Forward_Custom_Headers() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardHeaders = true }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("X-Custom-Header", "custom-value");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Contain("X-Custom-Header");
        response.Content.Should().Contain("custom-value");
    }

    // ─── ForwardAuthorization = true (positive test) ────────────────────

    [Fact]
    public async Task ForwardAuthorization_True_Should_Forward_Authorization_Header() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardAuthorization = true }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-with-status?status=302");
        request.AddHeader("Authorization", "Bearer keep-me");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Contain("Bearer keep-me");
    }

    // ─── ForwardBody = false ────────────────────────────────────────────

    [Fact]
    public async Task ForwardBody_False_Should_Drop_Body_Even_When_Verb_Preserved() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardBody = false }
        };
        using var client = new RestClient(options);

        // 307 preserves verb, but ForwardBody=false should still drop the body
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
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardBody = true }
        };
        using var client = new RestClient(options);

        // 307 preserves verb, ForwardBody=true (default) should keep the body
        var request = new RestRequest("/redirect-with-status?status=307", Method.Post);
        request.AddJsonBody(new { data = "keep-me" });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(response.Content!);
        doc.RootElement.GetProperty("Method").GetString().Should().Be("POST");
        doc.RootElement.GetProperty("Body").GetString().Should().Contain("keep-me");
    }

    // ─── ForwardQuery ───────────────────────────────────────────────────

    [Fact]
    public async Task ForwardQuery_True_Should_Carry_Query_When_Redirect_Has_No_Query() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardQuery = true }
        };
        using var client = new RestClient(options);

        // /redirect-no-query redirects to /echo-request with no query in the Location header
        var request = new RestRequest("/redirect-no-query");
        request.AddQueryParameter("foo", "bar");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The original query should be carried to the redirect target
        response.ResponseUri!.Query.Should().Contain("foo=bar");
    }

    [Fact]
    public async Task ForwardQuery_False_Should_Not_Carry_Query_To_Redirect() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions { ForwardQuery = false }
        };
        using var client = new RestClient(options);

        var request = new RestRequest("/redirect-no-query");
        request.AddQueryParameter("foo", "bar");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The original query should NOT be carried to the redirect target
        (response.ResponseUri!.Query ?? "").Should().NotContain("foo=bar");
    }

    // ─── RedirectStatusCodes customization ──────────────────────────────

    [Fact]
    public async Task Custom_RedirectStatusCodes_Should_Follow_Custom_Code() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions {
                RedirectStatusCodes = [HttpStatusCode.Found, (HttpStatusCode)399]
            }
        };
        using var client = new RestClient(options);

        // 399 is not a standard redirect, but we added it to the list
        var request  = new RestRequest("/redirect-custom-status?status=399");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "399 should be treated as a redirect because it's in RedirectStatusCodes");
    }

    [Fact]
    public async Task Custom_RedirectStatusCodes_Should_Not_Follow_Excluded_Code() {
        var options = new RestClientOptions(_server.Url!) {
            RedirectOptions = new RedirectOptions {
                // 302 is NOT in the custom list
                RedirectStatusCodes = [(HttpStatusCode)399]
            }
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/redirect-with-status?status=302");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Found,
            "302 should NOT be followed because it's not in the custom RedirectStatusCodes");
    }

    // ─── FollowRedirectsToInsecure ──────────────────────────────────────

    [Fact]
    public async Task FollowRedirectsToInsecure_False_Should_Block_Https_To_Http() {
        // Create an HTTPS WireMock server
        using var httpsServer = WireMockServer.Start(new WireMock.Settings.WireMockServerSettings {
            Port   = 0,
            UseSSL = true
        });

        httpsServer
            .Given(Request.Create().WithPath("/https-redirect"))
            .RespondWith(Response.Create().WithCallback(_ => new ResponseMessage {
                StatusCode = 302,
                Headers = new Dictionary<string, WireMockList<string>> {
                    // Redirect to plain HTTP server
                    ["Location"] = new(_server.Url + "/echo-request")
                }
            }));

        var options = new RestClientOptions(httpsServer.Url!) {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
            RedirectOptions = new RedirectOptions { FollowRedirectsToInsecure = false }
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/https-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect,
            "HTTPS to HTTP redirect should be blocked when FollowRedirectsToInsecure is false");
    }

    [Fact]
    public async Task FollowRedirectsToInsecure_True_Should_Allow_Https_To_Http() {
        // Create an HTTPS WireMock server
        using var httpsServer = WireMockServer.Start(new WireMock.Settings.WireMockServerSettings {
            Port   = 0,
            UseSSL = true
        });

        httpsServer
            .Given(Request.Create().WithPath("/https-redirect"))
            .RespondWith(Response.Create().WithCallback(_ => new ResponseMessage {
                StatusCode = 302,
                Headers = new Dictionary<string, WireMockList<string>> {
                    // Redirect to plain HTTP server
                    ["Location"] = new(_server.Url + "/echo-request")
                }
            }));

        var options = new RestClientOptions(httpsServer.Url!) {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
            RedirectOptions = new RedirectOptions { FollowRedirectsToInsecure = true }
        };
        using var client = new RestClient(options);

        var request  = new RestRequest("/https-redirect");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "HTTPS to HTTP redirect should be allowed when FollowRedirectsToInsecure is true");
    }

    public void Dispose() => _server.Dispose();
}
