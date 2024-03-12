using RestSharp.Tests.Integrated.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should().NotContain("'Accept':");
            content.Should().NotContain("'User-Agent':");
            // NOTE: This is expected to be there for normal HTTP purposes
            // and is expected to be re-added by the underlying HttpClient:
            content.Should().Contain("'Host': ");
            // NOTE: options.AutomaticDecompression controls
            // Accept-Encoding, so since we did nothing to change that
            // the underlying HttpClient will re-add this header:
            content.Should().Contain("'Accept-Encoding':");
            // These are expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");
            content.Should().Contain("'Authorization':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should().NotContain("'Accept':");
            content.Should().NotContain("'User-Agent':");
            content.Should().NotContain("'Authorization':");
            // NOTE: This is expected to be there for normal HTTP purposes
            // and is expected to be re-added by the underlying HttpClient:
            content.Should().Contain("'Host': ");
            // NOTE: options.AutomaticDecompression controls
            // Accept-Encoding, so since we did nothing to change that
            // the underlying HttpClient will re-add this header:
            content.Should().Contain("'Accept-Encoding':");
            // These are expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            content.Should().NotContain("'Accept':");
            content.Should().NotContain("'User-Agent':");
            content.Should().NotContain("'Authorization':");
            // This is expected due to redirection options for this test:
            content.Should().NotContain("'Cookie':");
            // NOTE: This is expected to be there for normal HTTP purposes
            // and is expected to be re-added by the underlying HttpClient:
            content.Should().Contain("'Host':");
            // NOTE: options.AutomaticDecompression controls
            // Accept-Encoding, so since we did nothing to change that
            // the underlying HttpClient will re-add this header:
            content.Should().Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should().NotContain("'Cookie':");
            // These should exist:
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Authorization':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");

            // Regardless of ForwardCookie, the cookie container is ALWAYS
            // updated:

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?blah=blah2");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");
            // These should exist:
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");
            // These should exist:
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers?url=%2fdump-headers#fragmentName");
            var content = response.Content;
            // This is expected due to redirection options for this test:
            content.Should().Contain("'Cookie':");
            // These should exist:
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
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
            response.ResponseUri.Should().Be($"{_baseUri}get-cookies#fragmentName");
            var content = response.Content;
            content.Should().Contain("redirectCookie=value1");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
        }

        [Fact]
        public async Task Can_RedirectBelowMaxRedirects_WithLoweredValue() {
            var options = NewOptions();
            options.RedirectOptions.MaxRedirects = 6;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "20");

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=15");
            HeaderParameter locationHeader = null;
            response.Headers.Should().Contain((header) => string.Compare(header.Name, "Location", StringComparison.InvariantCultureIgnoreCase) == 0);
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

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "20");

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=1");
            var content = response.Content;
            content.Should().Contain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_RedirectAtMaxRedirects() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "50");

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=1");
            var content = response.Content;
            content.Should().Contain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_StopRedirectAboveMaxRedirectDefault() {
            var options = NewOptions();
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "51");

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=2");
            var content = response.Content;
            content.Should().NotContain("Stopped redirection countdown!");
        }

        [Fact]
        public async Task Can_StopRedirectAboveMaxRedirectSet() {
            var options = NewOptions();
            options.RedirectOptions.MaxRedirects = 5;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest("/redirect-countdown") {
                Method = Method.Get,
            };
            request.AddQueryParameter("n", "6");

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}redirect-countdown?n=2");
            var content = response.Content;
            content.Should().NotContain("Stopped redirection countdown!");
        }

        // Custom logic that can either override or extends the .NET validation logic
        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        [Fact]
        public async Task Can_FailToRedirectToInsecureUrl() {
            var options = NewOptions();
            options.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest($"{_baseSecureUri}redirect-insecure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().NotBe($"{_baseUri}dump-headers");
            response.ResponseUri.Should().Be($"{_baseSecureUri}redirect-insecure");
            HeaderParameter locationHeader = null;
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

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest($"{_baseSecureUri}redirect-insecure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
            response.ResponseUri.Should().NotBe($"{_baseSecureUri}redirect-insecure");
        }

        [Fact]
        public async Task Can_RedirectToSecureUrl() {
            var options = NewOptions();
            options.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;
            var client = new RestClient(options);

            // This request sets cookies and redirects to url param value
            // if supplied, otherwise redirects to /get-cookies
            var request = new RestRequest($"{_baseUri}redirect-secure") {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);
            response.ResponseUri.Should().Be($"{_baseSecureUri}dump-headers");
            response.ResponseUri.Should().NotBe($"{_baseUri}redirect-insecure");
            var content = response.Content;
            content.Should().Contain("'Accept':");
            content.Should().Contain("'User-Agent':");
            content.Should().Contain("'Host':");
            content.Should().Contain("'Accept-Encoding':");
        }
    }
}
