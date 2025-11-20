namespace RestSharp.Tests;

public partial class UrlBuilderTests {
    [Fact]
    public void GET_with_empty_base_and_query_parameters_without_encoding() {
        var request = new RestRequest($"{Base}/{Resource}?param1=value1")
            .AddQueryParameter("foo", "bar,baz", false);
        var expected = new Uri($"{Base}/{Resource}?param1=value1&foo=bar,baz");

        using var client = new RestClient();

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_base_and_resource_containing_tokens() {
        var request = new RestRequest($"{Base}/{Resource}/{{foo}}");
        request.AddUrlSegment("foo", "bar");

        using var client = new RestClient();

        var expected = new Uri($"{Base}/{Resource}/bar");
        var output   = client.BuildUri(request);

        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request() {
        var request  = new RestRequest();
        var expected = new Uri(Base);

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request_and_bare_hostname() {
        var request  = new RestRequest();
        var expected = new Uri(Base);

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request_and_query_parameters_without_encoding() {
        var request = new RestRequest();
        request.AddQueryParameter("foo", "bar,baz", false);
        var expected = new Uri($"{Base}/{Resource}?param1=value1&foo=bar,baz");

        using var client = new RestClient($"{Base}/{Resource}?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Invalid_Url_string_throws_exception()
        => Assert.Throws<UriFormatException>(() => { _ = new RestClient("invalid url"); }
        );

    [Fact]
    public void GET_with_leading_slash() {
        var request  = new RestRequest($"/{Resource}");
        var expected = new Uri($"{Base}/{Resource}");

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_leading_slash_and_baseurl_trailing_slash() {
        var request = new RestRequest($"/{Resource}");
        request.AddParameter("foo", "bar");
        var expected = new Uri($"{Base}/{Resource}?foo=bar");

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_multiple_instances_of_same_key() {
        var request = new RestRequest("v1/people/~/network/updates");
        request.AddParameter("type", "STAT");
        request.AddParameter("type", "PICT");
        request.AddParameter("count", "50");
        request.AddParameter("start", "50");
        var expected = new Uri("https://api.linkedin.com/v1/people/~/network/updates?type=STAT&type=PICT&count=50&start=50");

        using var client = new RestClient("https://api.linkedin.com");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_resource_containing_null_token() {
        var request = new RestRequest($"/{Resource}/{{foo}}");
        request.AddUrlSegment("foo", null);

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(new($"{Base}/{Resource}/"), output);
    }

    [Fact]
    public void GET_with_resource_containing_slashes() {
        var request  = new RestRequest($"{Resource}/foo");
        var expected = new Uri($"{Base}/{Resource}/foo");

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_resource_containing_tokens() {
        var request = new RestRequest($"{Resource}/{{foo}}");
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri($"{Base}/{Resource}/bar");

        using var client = new RestClient(new Uri(Base));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Uri_and_resource_containing_tokens() {
        var request = new RestRequest($"/{{foo}}/{Resource}/{{baz}}");
        request.AddUrlSegment("foo", "bar");
        request.AddUrlSegment("baz", "bat");
        var expected = new Uri($"{Base}/bar/{Resource}/bat");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Uri_containing_tokens() {
        var request = new RestRequest();
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri(Base);

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Url_string_and_resource_containing_tokens() {
        var request = new RestRequest($"{Resource}/{{baz}}");
        request.AddUrlSegment("foo", "bar");
        request.AddUrlSegment("baz", "bat");
        var expected = new Uri($"{Base}/bar/{Resource}/bat");

        using var client = new RestClient($"{Base}/{{foo}}");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Url_string_containing_tokens() {
        var request = new RestRequest();
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri($"{Base}/bar");

        using var client = new RestClient($"{Base}/{{foo}}");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_wth_trailing_slash_and_query_parameters() {
        var request = new RestRequest($"/{Resource}/");
        request.AddParameter("foo", "bar");
        var expected = new Uri($"{Base}/{Resource}/?foo=bar");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Multiple_query_parameters_with_same_name() {
        var request = new RestRequest($"/{Resource}/")
            .AddQueryParameter("foo", "bar")
            .AddQueryParameter("foo", "baz");
        var expected = new Uri($"{Base}/{Resource}/?foo=bar&foo=baz");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Query_parameter_with_non_string_value() {
        var request = new RestRequest($"/{Resource}/")
            .AddQueryParameter("foo", 123)
            .AddQueryParameter("bar", true);
        var expected = new Uri($"{Base}/{Resource}/?foo=123&bar=True");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }
}