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
        public async Task Can_RedirectForwardHeadersFalse_DropHeaders() {
            var options = NewOptions();
            options.RedirectOptions.ForwardHeaders = false;
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
            content.Should().NotContain("'Accept':");
            content.Should().NotContain("'Host': ");
            content.Should().NotContain("'User-Agent':");
            content.Should().NotContain("'Accept-Encoding':");
            content.Should().NotContain("'Cookie':");

            // Verify the cookie exists from the redirected get:
            response.Cookies.Count.Should().BeGreaterThan(0).And.Be(1);
            response.Cookies[0].Name.Should().Be("redirectCookie");
            response.Cookies[0].Value.Should().Be("value1");
        }

    }
}
