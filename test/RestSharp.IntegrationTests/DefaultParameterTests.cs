using System.Net;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class DefaultParameterTests : IDisposable {
    readonly SimpleServer _server;

    public DefaultParameterTests() => _server = SimpleServer.Create(RequestHandler.Handle);

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task Should_add_default_and_request_query_get_parameters() {
        var client  = new RestClient(_server.Url).AddDefaultParameter("foo", "bar", ParameterType.QueryString);
        var request = new RestRequest().AddParameter("foo1", "bar1", ParameterType.QueryString);

        await client.GetAsync(request);

        var query = RequestHandler.Url.Query;
        query.Should().Contain("foo=bar");
        query.Should().Contain("foo1=bar1");
    }

    [Fact]
    public async Task Should_add_default_and_request_url_get_parameters() {
        var client  = new RestClient($"{_server.Url}{{foo}}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
        var request = new RestRequest("{foo1}").AddParameter("foo1", "bar1", ParameterType.UrlSegment);

        await client.GetAsync(request);

        RequestHandler.Url.Segments.Should().BeEquivalentTo("/", "bar/", "bar1");
    }

    [Fact]
    public async Task Should_not_throw_exception_when_name_is_null() {
        var client  = new RestClient($"{_server.Url}{{foo}}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
        var request = new RestRequest("{foo1}").AddParameter(null, "value", ParameterType.RequestBody);

        await client.ExecuteAsync(request);
    }

    static class RequestHandler {
        public static Uri Url { get; private set; }

        public static void Handle(HttpListenerContext context) {
            Url = context.Request.Url;
            Handlers.Echo(context);
        }
    }
}