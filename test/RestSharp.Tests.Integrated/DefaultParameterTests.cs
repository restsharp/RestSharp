using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class DefaultParameterTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    readonly RequestBodyCapturer _capturer       = server.ConfigureBodyCapturer(Method.Get, false);
    readonly RequestBodyCapturer _capturerOnPath = server.ConfigureBodyCapturer(Method.Get);

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

    [Fact]
    public async Task Should_not_encode_pipe_character_when_encode_is_false() {
        using var client = new RestClient(server.Url!);

        var request = new RestRequest("capture");
        request.AddQueryParameter("ids", "in:001|116", false);

        await client.ExecuteAsync(request);

        var query = _capturer.RawUrl.Split('?')[1];
        query.Should().Contain("ids=in:001|116");
    }

    [Fact]
    public async Task Should_include_multiple_default_query_params_with_same_name() {
        using var client = new RestClient(
            new RestClientOptions(server.Url!) { AllowMultipleDefaultParametersWithSameName = true }
        );
        client.AddDefaultParameter("filter", "active", ParameterType.QueryString);
        client.AddDefaultParameter("filter", "verified", ParameterType.QueryString);

        var request = new RestRequest("capture");
        await client.GetAsync(request);

        var query = _capturerOnPath.Url!.Query;
        query.Should().Contain("filter=active");
        query.Should().Contain("filter=verified");
    }

    [Fact]
    public async Task Should_include_default_query_params_in_BuildUriString_without_executing() {
        using var client = new RestClient(server.Url!);
        client.AddDefaultParameter("foo", "bar", ParameterType.QueryString);

        var request = new RestRequest("resource");
        var uri     = client.BuildUriString(request);

        uri.Should().Contain("foo=bar");
    }

    [Fact]
    public async Task Should_not_permanently_mutate_request_parameters_after_execute() {
        using var client = new RestClient(server.Url!);
        client.AddDefaultParameter("default_key", "default_val", ParameterType.QueryString);

        var request      = new RestRequest("capture");
        var paramsBefore = request.Parameters.Count;

        await client.GetAsync(request);

        // Request parameters should not have been mutated by the execution.
        request.Parameters.Count.Should().Be(paramsBefore);

        // Now replace the default parameter with a different value.
        client.DefaultParameters.ReplaceParameter(new QueryParameter("default_key", "updated_val"));

        await client.GetAsync(request);

        // The second execution should use the updated default value, not the stale one.
        var query = _capturerOnPath.Url!.Query;
        query.Should().Contain("default_key=updated_val");
        query.Should().NotContain("default_key=default_val");
    }

    [Fact]
    public async Task Should_include_default_params_in_merged_parameters_on_response() {
        using var client = new RestClient(server.Url!);
        client.AddDefaultParameter("default_key", "default_val", ParameterType.QueryString);

        var request  = new RestRequest("capture").AddQueryParameter("req_key", "req_val");
        var response = await client.ExecuteAsync(request);

        var defaultParam = response.MergedParameters
            .FirstOrDefault(p => p.Name == "default_key" && p.Type == ParameterType.QueryString);
        defaultParam.Should().NotBeNull();
        defaultParam!.Value.Should().Be("default_val");

        var requestParam = response.MergedParameters
            .FirstOrDefault(p => p.Name == "req_key" && p.Type == ParameterType.QueryString);
        requestParam.Should().NotBeNull();
        requestParam!.Value.Should().Be("req_val");
    }
}