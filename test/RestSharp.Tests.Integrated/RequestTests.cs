using System.Net;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class AsyncTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;

    public AsyncTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output  = output;
        _client  = new RestClient(fixture.Server.Url);
    }

    class Response {
        public string Message { get; set; }
    }

    [Fact]
    public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var request = new RestRequest("success");

        request.OnBeforeDeserialization += _ => throw new Exception(exceptionMessage);

        var response = await _client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_ExecuteGetAsync_With_Response_Type() {
        var request  = new RestRequest("success");
        var response = await _client.ExecuteAsync<Response>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Perform_GET_Async() {
        const string val = "Basic async test";

        var request = new RestRequest($"echo?msg={val}");

        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be(val);
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Request_Cookies() {
        var request = new RestRequest("get-cookies") {
            CookieContainer = new CookieContainer()
        };
        request.CookieContainer.Add(new Cookie("cookie", "value", null, _client.Options.BaseUrl.Host));
        request.CookieContainer.Add(new Cookie("cookie2", "value2", null, _client.Options.BaseUrl.Host));
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("[\"cookie=value\",\"cookie2=value2\"]");
    }

    [Fact]
    public async Task Can_Perform_GET_Async_With_Response_Cookies() {
        var request = new RestRequest("set-cookies");
        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be("success");

        // Check we got all our cookies
        var domain = _client.Options.BaseUrl.Host;
        var cookie = response.Cookies!.First(p => p.Name == "cookie1");
        Assert.Equal("value1", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(domain, cookie.Domain);
        Assert.Equal(DateTime.MinValue, cookie.Expires);
        Assert.False(cookie.HttpOnly);

        // Cookie 2 should vanish as the path will not match
        cookie = response.Cookies!.FirstOrDefault(p => p.Name == "cookie2");
        Assert.Null(cookie);

        // Check cookie3 has a valid expiration
        cookie = response.Cookies!.First(p => p.Name == "cookie3");
        Assert.Equal("value3", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(domain, cookie.Domain);
        Assert.True(cookie.Expires > DateTime.Now);

        // Check cookie4 has a valid expiration
        cookie = response.Cookies!.First(p => p.Name == "cookie4");
        Assert.Equal("value4", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(domain, cookie.Domain);
        Assert.True(cookie.Expires > DateTime.Now);

        // Cookie 5 should vanish as the request is not SSL
        cookie = response.Cookies!.FirstOrDefault(p => p.Name == "cookie5");
        Assert.Null(cookie);

        // Check cookie6 should be http only
        cookie = response.Cookies!.First(p => p.Name == "cookie6");
        Assert.Equal("value6", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(domain, cookie.Domain);
        Assert.Equal(DateTime.MinValue, cookie.Expires);
        Assert.True(cookie.HttpOnly);
    }

    [Fact]
    public async Task Can_Timeout_GET_Async() {
        var request = new RestRequest("timeout").AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 200;

        var response = await _client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_Delete_With_Response_Type() {
        var request  = new RestRequest("delete");
        var response = await _client.ExecuteAsync<Response>(request, Method.Delete);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Delete_With_Response_Type_using_extension() {
        var request  = new RestRequest("delete");
        var response = await _client.DeleteAsync<Response>(request);

        response!.Message.Should().Be("Works!");
    }
}