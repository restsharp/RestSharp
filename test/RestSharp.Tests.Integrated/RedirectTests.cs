//  Copyright (c) .NET Foundation and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Net;
using RestSharp.Tests.Integrated.Server;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class RedirectTests {
    readonly RestClient _client;
    readonly string     _host;

    public RedirectTests(TestServerFixture fixture) {
        var options = new RestClientOptions(fixture.Server.Url) {
            FollowRedirects = true
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
    public async Task Can_Perform_GET_Async_With_Request_Cookies_And_RedirectCookie() {
        var request = new RestRequest("get-cookies-redirect") {
            CookieContainer = new CookieContainer(),
        };
        request.AddQueryParameter("url", "set-cookies");
        request.CookieContainer.Add(new Cookie("cookie", "value", null, _host));
        request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _host));
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Contain("success");
        request.CookieContainer!.Count.Should().Be(9);
        request.CookieContainer!.GetAllCookies().Should()
                .ContainCookieWithNameAndValue("cookie", "value")
                .And.ContainCookieWithNameAndValue("cookie2", "value2")
                .And.ContainCookieWithNameAndValue("redirectCookie", "value1")
                .And.ContainCookieWithNameAndValue("cookie1", "value1")
                .And.ContainCookieWithNameAndValue("cookie2", "value2")
                .And.ContainCookieWithNameAndValue("cookie3", "value3")
                .And.ContainCookieWithNameAndValue("cookie4", "value4")
                .And.ContainCookieWithNameAndValue("cookie5", "value5")
                .And.ContainCookieWithNameAndValue("cookie6", "value6");
    }

    [Fact]
    public async Task Can_Perform_POST_Async_With_RedirectionResponse_Cookies() {
        var request = new RestRequest("/post/set-cookie-redirect") {
            Method = Method.Post,
        };

        var response = await _client.ExecuteAsync(request);

        // Verify the cookie exists from the POST:
        response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
        response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
        // Make sure the redirected location spits out the correct content:
        response.Content.Should().Be("[\"redirectCookie=value1\"]", "was successfully redirected to get-cookies");
    }

    [Fact]
    public async Task Can_Perform_POST_Async_With_SeeOtherRedirectionResponse_Cookies() {
        var request = new RestRequest("/post/set-cookie-seeother") {
            Method = Method.Post,
        };

        var response = await _client.ExecuteAsync(request);

        // Verify the cookie exists from the POST:
        response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
        response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "seeOtherValue1");
        // Make sure the redirected location spits out the correct content:
        response.Content.Should().Be("[\"redirectCookie=seeOtherValue1\"]", "was successfully redirected to get-cookies");
    }

    [Fact]
    public async Task Can_Perform_PUT_Async_With_RedirectionResponse_Cookies() {
        var request = new RestRequest("/put/set-cookie-redirect") {
            Method = Method.Put,
        };

        var response = await _client.ExecuteAsync(request);

        // Verify the cookie exists from the PUT:
        response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
        response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "putCookieValue1");
        // However, the redirection status code should be a 405 (Method Not Allowed):
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Can_ForwardHeadersTrue_OnRedirect() {
        // This request sets cookies and redirects to url param value
        // if supplied, otherwise redirects to /get-cookies
        var request = new RestRequest("/get-cookies-redirect") {
            Method = Method.Get,
        };
        request.AddQueryParameter("url", "/dump-headers");

        var response = await _client.ExecuteAsync(request);
        response.ResponseUri.Should().Be($"{_client.Options.BaseUrl}dump-headers?url=%2fdump-headers");
        var content = response.Content;
        content.Should()
            .Contain("'Accept':")
            .And.Contain($"'Host': {_client.Options.BaseHost}")
            .And.Contain("'User-Agent':")
            .And.Contain("'Accept-Encoding':")
            .And.Contain("'Cookie':");

        // Verify the cookie exists from the redirected get:
        response.Cookies!.Count.Should().BeGreaterThan(0).And.Be(1);
        response.Cookies.Should().ContainCookieWithNameAndValue("redirectCookie", "value1");
    }

    // Needed tests:
    //Test: ForwardBody = true (default, might not need test)
    //Test: ForwardBody = false
    //Test: ForceForwardBody = false (default, might not need test)
    //Test: Altered Redirect Status Codes list


    class Response {
        public string? Message { get; set; }
    }
}
