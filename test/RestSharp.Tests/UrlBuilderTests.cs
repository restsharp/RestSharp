using System.Text;

namespace RestSharp.Tests;

/// <summary>
/// Note: These tests do not handle QueryString building, which is handled in Http, not RestClient
/// </summary>
public class UrlBuilderTests {
    [Fact]
    public void GET_with_empty_base_and_query_parameters_without_encoding() {
        var request = new RestRequest("http://example.com/resource?param1=value1")
            .AddQueryParameter("foo", "bar,baz", false);
        var expected = new Uri("http://example.com/resource?param1=value1&foo=bar,baz");

        using var client = new RestClient();

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_base_and_resource_containing_tokens() {
        var request = new RestRequest("http://example.com/resource/{foo}");
        request.AddUrlSegment("foo", "bar");

        using var client = new RestClient();

        var expected = new Uri("http://example.com/resource/bar");
        var output   = client.BuildUri(request);

        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request() {
        var request  = new RestRequest();
        var expected = new Uri("http://example.com/");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request_and_bare_hostname() {
        var request  = new RestRequest();
        var expected = new Uri("http://example.com/");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_empty_request_and_query_parameters_without_encoding() {
        var request = new RestRequest();
        request.AddQueryParameter("foo", "bar,baz", false);
        var expected = new Uri("http://example.com/resource?param1=value1&foo=bar,baz");

        using var client = new RestClient("http://example.com/resource?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Invalid_Url_string_throws_exception()
        => Assert.Throws<UriFormatException>(
            () => {
                var unused = new RestClient("invalid url");
            }
        );

    [Fact]
    public void GET_with_leading_slash() {
        var request  = new RestRequest("/resource");
        var expected = new Uri("http://example.com/resource");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_leading_slash_and_baseurl_trailing_slash() {
        var request = new RestRequest("/resource");
        request.AddParameter("foo", "bar");
        var expected = new Uri("http://example.com/resource?foo=bar");

        using var client = new RestClient(new Uri("http://example.com"));

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
        var request = new RestRequest("/resource/{foo}");
        Assert.Throws<ArgumentNullException>(() => request.AddUrlSegment("foo", null));
    }

    [Fact]
    public void GET_with_resource_containing_slashes() {
        var request  = new RestRequest("resource/foo");
        var expected = new Uri("http://example.com/resource/foo");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_resource_containing_tokens() {
        var request = new RestRequest("resource/{foo}");
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri("http://example.com/resource/bar");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Uri_and_resource_containing_tokens() {
        var request = new RestRequest("resource/{baz}");
        request.AddUrlSegment("foo", "bar");
        request.AddUrlSegment("baz", "bat");
        var expected = new Uri("http://example.com/bar/resource/bat");

        using var client = new RestClient(new Uri("http://example.com/{foo}"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Uri_containing_tokens() {
        var request = new RestRequest();
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri("http://example.com/bar");

        using var client = new RestClient(new Uri("http://example.com/{foo}"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Url_string_and_resource_containing_tokens() {
        var request = new RestRequest("resource/{baz}");
        request.AddUrlSegment("foo", "bar");
        request.AddUrlSegment("baz", "bat");
        var expected = new Uri("http://example.com/bar/resource/bat");

        using var client = new RestClient("http://example.com/{foo}");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_with_Url_string_containing_tokens() {
        var request = new RestRequest();
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri("http://example.com/bar");

        using var client = new RestClient("http://example.com/{foo}");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void GET_wth_trailing_slash_and_query_parameters() {
        var request = new RestRequest("/resource/");
        request.AddParameter("foo", "bar");
        var expected = new Uri("http://example.com/resource/?foo=bar");

        using var client = new RestClient("http://example.com");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_leading_slash() {
        var request  = new RestRequest("/resource", Method.Post);
        var expected = new Uri("http://example.com/resource");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_leading_slash_and_baseurl_trailing_slash() {
        var request  = new RestRequest("/resource", Method.Post);
        var expected = new Uri("http://example.com/resource");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_querystring_containing_tokens() {
        var request = new RestRequest("resource", Method.Post);
        request.AddParameter("foo", "bar", ParameterType.QueryString);
        var expected = new Uri("http://example.com/resource?foo=bar");

        using var client = new RestClient("http://example.com");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_resource_containing_slashes() {
        var request  = new RestRequest("resource/foo", Method.Post);
        var expected = new Uri("http://example.com/resource/foo");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void POST_with_resource_containing_tokens() {
        var request = new RestRequest("resource/{foo}", Method.Post);
        request.AddUrlSegment("foo", "bar");
        var expected = new Uri("http://example.com/resource/bar");

        using var client = new RestClient(new Uri("http://example.com"));

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_add_parameter_if_it_is_new() {
        var request = new RestRequest();
        request.AddOrUpdateParameter("param2", "value2");
        request.AddOrUpdateParameter("param3", "value3");
        var expected = new Uri("http://example.com/resource?param1=value1&param2=value2&param3=value3");

        using var client = new RestClient("http://example.com/resource?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_build_uri_using_selected_encoding() {
        var request = new RestRequest();
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        request.AddOrUpdateParameter("town", "Hillerød");
        var expectedDefaultEncoding  = new Uri("http://example.com/resource?town=Hiller%C3%B8d");
        var expectedIso89591Encoding = new Uri("http://example.com/resource?town=Hiller%f8d");

        using var client1 = new RestClient(new RestClientOptions("http://example.com/resource"));
        Assert.Equal(expectedDefaultEncoding, client1.BuildUri(request));

        using var client2 = new RestClient(new RestClientOptions("http://example.com/resource") { Encoding = Encoding.GetEncoding("ISO-8859-1") });
        Assert.Equal(expectedIso89591Encoding, client2.BuildUri(request));
    }

    [Fact]
    public void Should_build_uri_with_resource_full_uri() {
        var request  = new RestRequest("https://www.example1.com/connect/authorize");
        var expected = new Uri("https://www.example1.com/connect/authorize");

        using var client = new RestClient("https://www.example1.com/");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_encode_colon() {
        var request = new RestRequest();
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        request.AddOrUpdateParameter("parameter", "some:value");

        using var client = new RestClient("http://example.com/resource");

        var expectedDefaultEncoding = new Uri("http://example.com/resource?parameter=some%3avalue");
        Assert.Equal(expectedDefaultEncoding, client.BuildUri(request));
    }

    [Fact]
    public void Should_not_duplicate_question_mark() {
        var request = new RestRequest();
        request.AddParameter("param2", "value2");
        var expected = new Uri("http://example.com/resource?param1=value1&param2=value2");

        using var client = new RestClient("http://example.com/resource?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_not_touch_request_url() {
        const string baseUrl    = "http://rs.test.org";
        const string requestUrl = "reportserver?/Prod/Report";

        var req = new RestRequest(requestUrl, Method.Post);

        using var client = new RestClient(baseUrl);

        var resultUrl = client.BuildUri(req).ToString();
        resultUrl.Should().Be($"{baseUrl}/{requestUrl}");
    }

    [Fact]
    public void Should_update_parameter_if_it_already_exists() {
        var request = new RestRequest();
        request.AddOrUpdateParameter("param2", "value2");
        request.AddOrUpdateParameter("param2", "value2-1");
        var expected = new Uri("http://example.com/resource?param1=value1&param2=value2-1");

        using var client = new RestClient("http://example.com/resource?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_use_ipv6_address() {
        var baseUrl = new Uri("https://[fe80::290:e8ff:fe8b:2537%en10]:8443");
        var request = new RestRequest("api/v1/auth");

        using var client = new RestClient(baseUrl);

        var actual = client.BuildUri(request);
        actual.HostNameType.Should().Be(UriHostNameType.IPv6);
        actual.AbsoluteUri.Should().Be("https://[fe80::290:e8ff:fe8b:2537]:8443/api/v1/auth");
    }
}