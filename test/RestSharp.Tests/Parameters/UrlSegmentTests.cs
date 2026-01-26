namespace RestSharp.Tests.Parameters;

public class UrlSegmentTests {
    const string BaseUrlNoTrail = "http://localhost:8888";
    const string BaseUrl        = $"{BaseUrlNoTrail}/";

    [Fact]
    public void AddUrlSegmentWithInt() {
        const string name = "foo";

        var request  = new RestRequest().AddUrlSegment(name, 1);
        var actual   = request.Parameters.FirstOrDefault(x => x.Name == name);
        var expected = new UrlSegmentParameter(name, "1");

        expected.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void AddUrlSegmentModifiesUrlSegmentWithInt() {
        const string name            = "foo";
        const string pathTemplate    = "/{0}/resource";
        const int    urlSegmentValue = 1;

        var path     = string.Format(pathTemplate, $"{{{name}}}");
        var request  = new RestRequest(path).AddUrlSegment(name, urlSegmentValue);
        var expected = $"{BaseUrlNoTrail}{string.Format(pathTemplate, urlSegmentValue)}";

        using var client = new RestClient(BaseUrl);
        var       actual = client.BuildUriString(request);

        expected.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void AddUrlSegmentModifiesUrlSegmentWithString() {
        const string name            = "foo";
        const string pathTemplate    = "/{0}/resource";
        const string urlSegmentValue = "bar";

        var path     = string.Format(pathTemplate, $"{{{name}}}");
        var request  = new RestRequest(path).AddUrlSegment(name, urlSegmentValue);
        var expected = $"{BaseUrlNoTrail}{string.Format(pathTemplate, urlSegmentValue)}";

        using var client = new RestClient(BaseUrl);

        var actual = client.BuildUriString(request);

        expected.Should().BeEquivalentTo(actual);
    }

    [Theory]
    [InlineData("bar%2fBAR")]
    [InlineData("bar%2FBAR")]
    public void UrlSegmentParameter_WithValueWithEncodedSlash_WillReplaceEncodedSlashByDefault(string inputValue) {
        var urlSegmentParameter = new UrlSegmentParameter("foo", inputValue);
        urlSegmentParameter.Value.Should().BeEquivalentTo("bar/BAR");
    }

    [Theory]
    [InlineData("bar%2fBAR")]
    [InlineData("bar%2FBAR")]
    public void UrlSegmentParameter_WithValueWithEncodedSlash_CanReplaceEncodedSlash(string inputValue) {
        var urlSegmentParameter = new UrlSegmentParameter("foo", inputValue, replaceEncodedSlash: true);
        urlSegmentParameter.Value.Should().BeEquivalentTo("bar/BAR");
    }

    [Theory]
    [InlineData("bar%2fBAR")]
    [InlineData("bar%2FBAR")]
    public void UrlSegmentParameter_WithValueWithEncodedSlash_CanLeaveEncodedSlash(string inputValue) {
        var urlSegmentParameter = new UrlSegmentParameter("foo", inputValue, replaceEncodedSlash: false);
        urlSegmentParameter.Value.Should().BeEquivalentTo(inputValue);
    }

    [Fact]
    public void AddSameUrlSegmentTwice_ShouldReplaceFirst() {
        const string host    = "https://api.example.com";
        var          client  = new RestClient();
        var          request = new RestRequest($"{host}/orgs/{{segment}}/something");
        request.AddUrlSegment("segment", 1);
        var url1 = client.BuildUriString(request);
        request.AddUrlSegment("segment", 2);
        var url2 = client.BuildUriString(request);

        url1.Should().Be($"{host}/orgs/1/something");
        url2.Should().Be($"{host}/orgs/2/something");
    }
}