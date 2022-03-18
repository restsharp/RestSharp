using RestSharp.Serializers.Json;

namespace RestSharp.Tests;

public class RestClientTests {
    const string BaseUrl = "http://localhost:8888/";

    [Theory]
    [InlineData(Method.Get, Method.Post)]
    [InlineData(Method.Post, Method.Get)]
    [InlineData(Method.Delete, Method.Get)]
    [InlineData(Method.Head, Method.Post)]
    [InlineData(Method.Put, Method.Patch)]
    [InlineData(Method.Patch, Method.Put)]
    [InlineData(Method.Post, Method.Put)]
    [InlineData(Method.Get, Method.Delete)]
    public async Task Execute_with_IRestRequest_and_Method_overrides_previous_request_method(Method reqMethod, Method overrideMethod) {
        var req    = new RestRequest("", reqMethod);
        var client = new RestClient(BaseUrl);

        await client.ExecuteAsync(req, overrideMethod);

        req.Method.Should().Be(overrideMethod);
    }

    [Fact]
    public void ConfigureHttp_will_set_proxy_to_null_with_no_exceptions_When_no_proxy_can_be_found() {
        var req    = new RestRequest();
        var client = new RestClient(new RestClientOptions(BaseUrl) { Proxy = null });

        client.ExecuteAsync(req);
    }

    [Fact]
    public void BuildUri_should_build_with_passing_link_as_Uri() {
        // arrange
        var relative    = new Uri("/foo/bar/baz", UriKind.Relative);
        var absoluteUri = new Uri(new Uri(BaseUrl), relative);
        var req         = new RestRequest(absoluteUri);

        // act
        var client   = new RestClient();
        var builtUri = client.BuildUri(req);

        // assert
        absoluteUri.Should().Be(builtUri);
    }

    [Fact]
    public void BuildUri_should_build_with_passing_link_as_Uri_with_set_BaseUrl() {
        // arrange
        var baseUrl  = new Uri(BaseUrl);
        var relative = new Uri("/foo/bar/baz", UriKind.Relative);
        var req      = new RestRequest(relative);

        // act
        var client   = new RestClient(baseUrl);
        var builtUri = client.BuildUri(req);

        // assert
        new Uri(baseUrl, relative).Should().Be(builtUri);
    }

    [Fact]
    public void UseJson_leaves_only_json_serializer() {
        // arrange
        var baseUrl  = new Uri(BaseUrl);

        // act
        var client   = new RestClient(baseUrl);
        client.UseJson();

        // assert
        Assert.Single(client.Serializers);
        Assert.True(client.Serializers.ContainsKey(DataFormat.Json));
    }

    [Fact]
    public void UseXml_leaves_only_json_serializer() {
        // arrange
        var baseUrl = new Uri(BaseUrl);

        // act
        var client = new RestClient(baseUrl);
        client.UseXml();

        // assert
        Assert.Single(client.Serializers);
        Assert.True(client.Serializers.ContainsKey(DataFormat.Xml));
    }

    [Fact]
    public void UseOnlySerializer_leaves_only_custom_serializer() {
        // arrange
        var baseUrl = new Uri(BaseUrl);

        // act
        var client = new RestClient(baseUrl);
        client.UseOnlySerializer(() => new SystemTextJsonSerializer());

        // assert
        Assert.Single(client.Serializers);
        Assert.True(client.Serializers.ContainsKey(DataFormat.Json));
    }
}