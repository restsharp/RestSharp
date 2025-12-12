using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class DefaultParameterTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    readonly RequestBodyCapturer _capturer = server.ConfigureBodyCapturer(Method.Get, false);

    [Fact]
    public async Task Should_add_default_and_request_query_get_parameters() {
        using var client  = new RestClient(server.Url!).AddDefaultParameter("foo", "bar", ParameterType.QueryString);
        var       request = new RestRequest().AddParameter("foo1", "bar1", ParameterType.QueryString);

        await client.GetAsync(request);

        var query = _capturer.Url!.Query;
        query.Should().Contain("foo=bar");
        query.Should().Contain("foo1=bar1");
    }

    [Fact]
    public async Task Should_add_default_and_request_url_get_parameters() {
        using var client  = new RestClient($"{server.Url}/{{foo}}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
        var       request = new RestRequest("{foo1}").AddParameter("foo1", "bar1", ParameterType.UrlSegment);

        await client.GetAsync(request);

        _capturer.Url!.Segments.Should().BeEquivalentTo("/", "bar/", "bar1");
    }

    [Fact]
    public async Task Should_not_throw_exception_when_name_is_null() {
        using var client  = new RestClient($"{server.Url}/request-echo").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
        var       request = new RestRequest("{foo1}").AddParameter(null, "value", ParameterType.RequestBody);

        await client.ExecuteAsync(request);
    }
}