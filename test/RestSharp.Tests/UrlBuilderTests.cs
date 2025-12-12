using System.Text;

namespace RestSharp.Tests;

/// <summary>
/// Note: These tests do not handle QueryString building, which is handled in Http, not RestClient
/// </summary>
public partial class UrlBuilderTests {
    const string Base     = "https://some.path";
    const string Resource = "resource";

    [Fact]
    public void Should_add_parameter_if_it_is_new() {
        var request = new RestRequest();
        request.AddOrUpdateParameter("param2", "value2");
        request.AddOrUpdateParameter("param3", "value3");
        var expected = new Uri($"{Base}/{Resource}?param1=value1&param2=value2&param3=value3");

        using var client = new RestClient($"{Base}/{Resource}?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_build_uri_using_selected_encoding() {
        var request = new RestRequest();
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        request.AddOrUpdateParameter("town", "Hillerød");
        var expectedDefaultEncoding  = new Uri($"{Base}/{Resource}?town=Hiller%C3%B8d");
        var expectedIso89591Encoding = new Uri($"{Base}/{Resource}?town=Hiller%f8d");

        using var client1 = new RestClient(new RestClientOptions($"{Base}/{Resource}"));
        Assert.Equal(expectedDefaultEncoding, client1.BuildUri(request));

        using var client2 = new RestClient(new RestClientOptions($"{Base}/{Resource}") { Encoding = Encoding.GetEncoding("ISO-8859-1") });
        Assert.Equal(expectedIso89591Encoding, client2.BuildUri(request));
    }

    [Fact]
    public void Should_build_uri_with_resource_full_uri() {
        var request  = new RestRequest($"{Base}/connect/authorize");
        var expected = new Uri($"{Base}/connect/authorize");

        using var client = new RestClient(Base);

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_encode_colon() {
        var request = new RestRequest();
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        request.AddOrUpdateParameter("parameter", "some:value");

        using var client = new RestClient($"{Base}/{Resource}");

        var expectedDefaultEncoding = new Uri($"{Base}/{Resource}?parameter=some%3avalue");
        Assert.Equal(expectedDefaultEncoding, client.BuildUri(request));
    }

    [Fact]
    public void Should_not_duplicate_question_mark() {
        var request = new RestRequest();
        request.AddParameter("param2", "value2");
        var expected = new Uri($"{Base}/{Resource}?param1=value1&param2=value2");

        using var client = new RestClient($"{Base}/{Resource}?param1=value1");

        var output = client.BuildUri(request);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Should_not_touch_request_url() {
        const string baseUrl    = "https://rs.test.org";
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
        var expected = new Uri($"{Base}/{Resource}?param1=value1&param2=value2-1");

        using var client = new RestClient($"{Base}/{Resource}?param1=value1");

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
    

    [Fact]
    public void BuildUri_should_build_with_passing_link_as_Uri() {
        // arrange
        var relative    = new Uri("/foo/bar/baz", UriKind.Relative);
        var absoluteUri = new Uri(new(Base), relative);
        var req         = new RestRequest(absoluteUri);

        // act
        using var client = new RestClient();

        var builtUri = client.BuildUri(req);

        // assert
        absoluteUri.Should().Be(builtUri);
    }

    [Fact]
    public void BuildUri_should_build_with_passing_link_as_Uri_with_set_BaseUrl() {
        // arrange
        var baseUrl  = new Uri(Base);
        var relative = new Uri("/foo/bar/baz", UriKind.Relative);
        var req      = new RestRequest(relative);

        // act
        using var client = new RestClient(baseUrl);

        var builtUri = client.BuildUri(req);

        // assert
        new Uri(baseUrl, relative).Should().Be(builtUri);
    }
}