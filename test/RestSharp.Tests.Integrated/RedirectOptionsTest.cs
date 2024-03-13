using RestSharp.Tests.Integrated.Server;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Integrated {
    [Collection(nameof(TestServerCollection))]
    public class RedirectOptionsTest {
        readonly string _host;
        readonly Uri _baseUri;
        readonly Uri _baseSecureUri;

        public RedirectOptionsTest(TestServerFixture fixture) {
            _baseUri = fixture.Server.Url;
            _baseSecureUri = fixture.Server.SecureUrl;
            _host = _baseUri.Host;
        }

        RestClientOptions NewOptions() {
            return new RestClientOptions(_baseUri);
        }

        [Fact]
        public async Task Can_RedirectForwardHeadersFalseWithAuthAndCookie_DropHeaders() {
            var options = NewOptions();
            options.RedirectOptions.ForwardHeaders = false;
            options.RedirectOptions.ForwardAuthorization = true;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "blah");
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should()
                .NotContain("'Accept':")
                .And.NotContain("'User-Agent':")
                // NOTE: This is expected to be there for normal HTTP purposes
                // and is expected to be re-added by the underlying HttpClient:
                .And.Contain("'Host': ")
                // NOTE: options.AutomaticDecompression controls
                // Accept-Encoding, so since we did nothing to change that
                // the underlying HttpClient will re-add this header:
                .And.Contain("'Accept-Encoding':")
                // These are expected due to redirection options for this test:
                .And.Contain("'Cookie':")
                .And.Contain("'Authorization':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectForwardHeadersFalseWithoutCookie_DropHeadersAndCookies() {
            var options = NewOptions();
            options.RedirectOptions.ForwardHeaders = false;
            options.RedirectOptions.ForwardCookies = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "blah");
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            // NOTE: This is expected to be there for normal HTTP purposes
            // and is expected to be re-added by the underlying HttpClient:
            content.Should()
                .Contain("'Host': ")
                // NOTE: options.AutomaticDecompression controls
                // Accept-Encoding, so since we did nothing to change that
                // the underlying HttpClient will re-add this header:
                .And.Contain("'Accept-Encoding':")
                // These are expected due to redirection options for this test:
                .And.NotContain("'Cookie':")
                .And.NotContain("'Accept':")
                .And.NotContain("'User-Agent':")
                .And.NotContain("'Authorization':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            // The cookies from get-cookies-redirect are placed in the cookie container
            // even though they aren't transmitted to the server on the redirect to dump-headers:
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectForwardHeadersFalseWithCookie_DropHeaders() {
            var options = NewOptions();
            options.RedirectOptions.ForwardHeaders = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "blah");
            request.AddQueryParameter("url", "/dump-headers");

            // These are required to make sure existing cookie headers are preserved
            // for this test:
            request.CookieContainer = new();
            request.CookieContainer.Add(new Cookie("cookie", "value", null, _host));
            request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _host));

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should()
                // These are expected due to redirection options for this test:
                .Contain("'Cookie':")
                .And.NotContain("'Accept':")
                .And.NotContain("'User-Agent':")
                .And.NotContain("'Authorization':")
                // NOTE: This is expected to be there for normal HTTP purposes
                // and is expected to be re-added by the underlying HttpClient:
                .And.Contain("'Host': ")
                // NOTE: options.AutomaticDecompression controls
                // Accept-Encoding, so since we did nothing to change that
                // the underlying HttpClient will re-add this header:
                .And.Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(3);
            // The cookies from get-cookies-redirect are placed in the cookie container
            // even though they aren't transmitted to the server on the redirect to dump-headers:
            response.Cookies.Should()
                .ContainCookieWithNameAndValue("redirectCookie", "value1")
                .And.ContainCookieWithNameAndValue("cookie", "value")
                .And.ContainCookieWithNameAndValue("cookie2", "value2");
        }

        [Fact]
        public async Task Can_RedirectForwardHeadersFalseWithoutCookie_DropHeaders() {
            var options = NewOptions();
            options.RedirectOptions.ForwardHeaders = false;
            options.RedirectOptions.ForwardCookies = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "blah");
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should()
                // This is expected due to redirection options for this test:
                .NotContain("'Accept':")
                .And.NotContain("'User-Agent':")
                .And.NotContain("'Authorization':")
                .And.NotContain("'Cookie':")
                // NOTE: This is expected to be there for normal HTTP purposes
                // and is expected to be re-added by the underlying HttpClient:
                .And.Contain("'Host':")
                // NOTE: options.AutomaticDecompression controls
                // Accept-Encoding, so since we did nothing to change that
                // the underlying HttpClient will re-add this header:
                .And.Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardCookieFalse() {
            var options = NewOptions();
            options.RedirectOptions.ForwardAuthorization = true;
            options.RedirectOptions.ForwardCookies = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "blah");
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should()
                .NotContain("'Cookie':")
                // These should exist:
                .And.Contain("'Accept':")
                .And.Contain("'User-Agent':")
                .And.Contain("'Authorization':")
                .And.Contain("'Host':")
                .And.Contain("'Accept-Encoding':");

            // Regardless of ForwardCookie, the cookie container is ALWAYS
            // updated:

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardQueryWithRedirectLocationContainingQuery() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddQueryParameter("url", "/dump-headers?blah=blah2");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?blah=blah2");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should()
                .Contain("'Cookie':")
                // These should exist:
                .And.Contain("'Accept':")
                .And.Contain("'User-Agent':")
                .And.Contain("'Host':")
                .And.Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardQueryFalse() {
            var options = NewOptions();
            options.RedirectOptions.ForwardQuery = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
            var content = response.Content;
            content.Should()
                // This is expected due to redirection options for this test:
                .Contain("'Cookie':")
                // These should exist:
                .And.Contain("'Accept':")
                .And.Contain("'User-Agent':")
                .And.Contain("'Host':")
                .And.Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardFragment() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect#fragmentName") {
                Method = Method.Get,
            };
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers#fragmentName");
            var content = response.Content;
            content.Should()
                // This is expected due to redirection options for this test:
                .Contain("'Cookie':")
                // These should exist:
                .And.Contain("'Accept':")
                .And.Contain("'User-Agent':")
                .And.Contain("'Host':")
                .And.Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardFragmentFalse() {
            var options = NewOptions();
            options.RedirectOptions.ForwardFragment = false;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect#fragmentName") {
                Method = Method.Get,
            };
            request.AddQueryParameter("url", "/dump-headers");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");
            // These should exist:
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectWithForwardFragmentWithoutQuery() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/get-cookies-redirect#fragmentName") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}get-cookies#fragmentName");
            var content = response.Content;
            content.Should().Contain("redirectCookie=value1");

            // Verify the cookie exists from the redirected get:
            response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        }

        [Fact]
        public async Task Can_RedirectBelowMaxRedirects_WithLoweredValue() {
            var options = NewOptions();
            options.RedirectOptions.MaxRedirects = 6;
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "20");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=15");
            HeaderParameter? locationHeader = null;
            response.Headers.Should()
                .Contain((header) => string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0);
            locationHeader = (from header in response.Headers
                              where string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0
                              select header).First();
            locationHeader.Value.Should().Be("/redirect-countdown?n=14");
            var content = response.Content;
            content.Should().NotContain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_RedirectBelowMaxRedirects_WithDefault() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "20");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=1");
            var content = response.Content;
            content.Should().Contain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_RedirectAtMaxRedirects() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "50");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=1");
            var content = response.Content;
            content.Should().Contain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_StopRedirectAboveMaxRedirectDefault() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "51");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=2");
            var content = response.Content;
            content.Should().NotContain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_StopRedirectAboveMaxRedirectSet() {
            var options = NewOptions();
            options.RedirectOptions.MaxRedirects = 5;
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "6");

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=2");
            var content = response.Content;
            content.Should().NotContain("Stopped redirection countdown!");
        }

        // Custom logic that can either override or extends the .NET validation logic
        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        [Fact]
        public async Task Can_FailToRedirectToInsecureUrl() {
            var options = NewOptions();
            options.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;
            var client = new RestClient(options);

            // This request redirects to insecure /dump-headers
            // if the redirection is allowed.
            var request = new RestRequest($"{_baseSecureUri}redirect-insecure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().NotBe($"{_baseUri}dump-headers");
            response.ResponseUri.Should().Be($"{_baseSecureUri}redirect-insecure");
            HeaderParameter? locationHeader = null;
            response.Headers.Should().Contain((header) => string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0);
            locationHeader = (from header in response.Headers
                              where string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0
                              select header).First();
            locationHeader.Value.Should().Be($"{_baseUri}dump-headers");
        }

        [Fact]
        public async Task Can_RedirectToInsecureUrlWithRedirectOption_True() {
            var options = NewOptions();
            options.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;
            options.RedirectOptions.FollowRedirectsToInsecure = true;
            var client = new RestClient(options);

            // This request redirects to insecure /dump-headers
            // if the redirection is allowed.
            var request = new RestRequest($"{_baseSecureUri}redirect-insecure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
            response.ResponseUri.Should().NotBe($"{_baseSecureUri}redirect-insecure");
        }

        [Fact]
        public async Task Can_RedirectToSecureUrl() {
            var options = NewOptions();
            options.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;
            var client = new RestClient(options);

            // This request redirects to secure /dump-headers
            // if the redirection is allowed.
            var request = new RestRequest($"{_baseUri}redirect-secure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseSecureUri}dump-headers");
            response.ResponseUri.Should().NotBe($"{_baseUri}redirect-insecure");
            var content = response.Content;
            content.Should()
                .Contain("'Accept':")
                .And.Contain("'User-Agent':")
                .And.Contain("'Host':")
                .And.Contain("'Accept-Encoding':");
        }

        [Fact]
        public async Task Can_NotFollowRedirect_WithRedirectOption_FollowRedirect_False() {
            var options = NewOptions();
            options.RedirectOptions.FollowRedirects = false;
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest($"{_baseUri}redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "17");
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=17");
            HeaderParameter? locationHeader = null;
            response.Headers.Should().Contain((header) => string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0);
            locationHeader = (from header in response.Headers
                              where string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0
                              select header).First();
            locationHeader.Value.Should().Be("/redirect-countdown?n=16");
        }

        [Fact]
        public async Task Can_NotFollowRedirect_WithOption_FollowRedirect_False() {
            var options = NewOptions();
            options.FollowRedirects = false;
            var client = new RestClient(options);

            // This request issues redirections to itself subracting 1
            // from n until n == 1.
            var request = new RestRequest($"{_baseUri}redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "17");
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=17");
            HeaderParameter? locationHeader = null;
            response.Headers.Should().Contain((header) => string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0);
            locationHeader = (from header in response.Headers
                              where string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0
                              select header).First();
            locationHeader.Value.Should().Be("/redirect-countdown?n=16");
        }

        [Fact]
        public async Task Can_NotAlterVerb_WithRedirectOption_AllowForcedRedirectVerbChange_False_WithStatusCode_302() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            // This is the setting for the test:
            options.RedirectOptions.AllowForcedRedirectVerbChange = false;
            var client = new RestClient(options);

            // This request issues redirections to the url parameter or /dump-headers
            // with a 302 status code.
            var request = new RestRequest($"{_baseUri}redirect-forcechangeverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("POST")
                .And.Contain("blah blah blah");
        }

        [Fact]
        public async Task Can_NotAlterVerb_WithRedirectOption_AllowRedirectMethodStatusCodeToAlterVerb_WithStatusCode_303() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            // This is the setting for the test:
            options.RedirectOptions.AllowRedirectMethodStatusCodeToAlterVerb = false;
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 303 status code.
            var request = new RestRequest($"{_baseUri}redirect-changeverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("POST")
                .And.Contain("blah blah blah");
        }

        [Fact]
        public async Task Can_AlterVerb_WithStatusCode302() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 302 status code.
            var request = new RestRequest($"{_baseUri}redirect-forcechangeverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("GET")
                .And.NotContain("blah blah blah", "Altered verbs MUST NOT foward along the body");
        }

        [Fact]
        public async Task Can_AlterVerb_WithStatusCode_303() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 303 status code.
            var request = new RestRequest($"{_baseUri}redirect-changeverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("GET")
                .And.NotContain("blah blah blah", "Altered verbs MUST NOT foward along the body");
        }

        [Fact]
        public async Task Can_RedirectWithoutChangingVerb_With_RedirectStatus_307() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 307 status code.
            var request = new RestRequest($"{_baseUri}redirect-keepverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("POST")
                .And.Contain("blah blah blah", "Altered verbs MUST NOT foward along the body");
        }

        [Fact]
        public async Task Can_RedirectWithCookies_HavingOptionLevel_CookieContainer() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            options.CookieContainer = new ();
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 307 status code.
            var request = new RestRequest($"{_baseUri}get-cookies-redirect") {
                Method = Method.Get,
            };
            request.AddQueryParameter("url", $"{_baseUri}set-cookies");
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}set-cookies");
            response.Cookies!.Count.Should().Be(5);
            response.Cookies.Should()
                .ContainCookieWithNameAndValue("redirectCookie", "value1")
                .And.ContainCookieWithNameAndValue("cookie1", "value1")
                // This cookie is excluded from the response CookieCollection because /path_extra
                // doesn't intersect with the ResponseUri.
                .And.NotContainCookieWithNameAndValue("cookie2", "value2")
                .And.ContainCookieWithNameAndValue("cookie3", "value3")
                .And.ContainCookieWithNameAndValue("cookie4", "value4")
                // This cookie is excluded from the response CookieCollection because
                // it was marked as secure.
                .And.NotContainCookieWithNameAndValue("cookie5", "value5")
                .And.ContainCookieWithNameAndValue("cookie6", "value6");
            verifyAllCookies(request.CookieContainer!.GetAllCookies());
            verifyAllCookies(options.CookieContainer!.GetAllCookies());

            void verifyAllCookies(CookieCollection cookies) {
                cookies.Should()
                    .ContainCookieWithNameAndValue("redirectCookie", "value1")
                    .And.ContainCookieWithNameAndValue("cookie1", "value1")
                    .And.ContainCookieWithNameAndValue("cookie2", "value2")
                    .And.ContainCookieWithNameAndValue("cookie3", "value3")
                    .And.ContainCookieWithNameAndValue("cookie4", "value4")
                    .And.ContainCookieWithNameAndValue("cookie5", "value5")
                    .And.ContainCookieWithNameAndValue("cookie6", "value6");
            }
        }

        [Fact]
        public async Task Can_AlterVerb_WithRedirectStatusCode_303_AndForwardBody() {
            var options = NewOptions();
            // NOTE: This isn't required, it just makes the test simpler:
            options.RedirectOptions.ForwardQuery = false;
            options.RedirectOptions.ForceForwardBody = true;
            var client = new RestClient(options);
            // This request issues redirections to the url parameter or /dump-headers
            // with a 303 status code.
            var request = new RestRequest($"{_baseUri}redirect-changeverb") {
                Method = Method.Post,
            };
            request.AddQueryParameter("url", $"{_baseUri}dump-request");
            request.AddStringBody("blah blah blah", DataFormat.None);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.ResponseUri.Should().Be($"{_baseUri}dump-request");
            var content = response.Content;
            content.Should().Contain("GET")
                .And.Contain("blah blah blah", "ForwardBody");
        }
    }
}
