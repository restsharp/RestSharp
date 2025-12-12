namespace RestSharp.Tests.Headers;

public class DefaultHeaderTests {
    const string BaseUrl = "http://localhost:8888/";
    
    [Fact]
    public void AddDefaultHeadersUsingDictionary() {
        var headers = new Dictionary<string, string> {
            { KnownHeaders.ContentType, ContentType.Json },
            { KnownHeaders.Accept, ContentType.Json },
            { KnownHeaders.ContentEncoding, "gzip, deflate" }
        };

        var expected = headers.Select(x => new HeaderParameter(x.Key, x.Value));

        using var client = new RestClient(BaseUrl);
        client.AddDefaultHeaders(headers);

        var actual = client.DefaultParameters.Select(x => x as HeaderParameter);
        expected.Should().BeSubsetOf(actual);
    }

}