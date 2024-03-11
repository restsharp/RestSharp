using RestSharp.Tests.Integrated.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Tests.Integrated {
    [Collection(nameof(TestServerCollection))]
    public class RedirectOptionsTest {
        readonly string _host;
        readonly Uri _baseUri;

        public RedirectOptionsTest(TestServerFixture fixture) {
            _baseUri = fixture.Server.Url;
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
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
            response.ResponseUri.Should().Be($"{_baseUri}dump-headers");
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
    }
}
