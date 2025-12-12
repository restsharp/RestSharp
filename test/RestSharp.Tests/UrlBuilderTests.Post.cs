namespace RestSharp.Tests;

public partial class UrlBuilderTests {
    [Fact]
    public void POST_with_leading_slash() {
        var request  = new RestRequest($"/{Resource}", Method.Post);
        var expected = new Uri($"{Base}/{Resource}");

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_leading_slash_and_baseurl_trailing_slash() {
        var request  = new RestRequest($"/{Resource}", Method.Post);
        var expected = new Uri($"{Base}/{Resource}");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_querystring_containing_tokens() {
        var request = new RestRequest(Resource, Method.Post);
        request.AddParameter("foo", "bar", ParameterType.QueryString);
        var expected = new Uri($"{Base}/{Resource}?foo=bar");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_resource_containing_slashes() {
        var request  = new RestRequest($"{Resource}/foo", Method.Post);
        var expected = new Uri($"{Base}/{Resource}/foo");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_resource_containing_tokens() {
        var request = new RestRequest($"{Resource}/{{foo}}", Method.Post);
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri($"{Base}/{Resource}/bar");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }
}