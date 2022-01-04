namespace RestSharp.Tests;

public class ParametersTests {
    const string BaseUrl = "http://localhost:8888/";

    [Fact]
    public void AddDefaultHeadersUsingDictionary() {
        var headers = new Dictionary<string, string> {
            { KnownHeaders.ContentType, "application/json" },
            { KnownHeaders.Accept, "application/json" },
            { KnownHeaders.ContentEncoding, "gzip, deflate" }
        };

        var expected = headers.Select(x => new HeaderParameter(x.Key, x.Value));

        var client  = new RestClient(BaseUrl);
        client.AddDefaultHeaders(headers);

        expected.Should().BeSubsetOf(client.DefaultParameters);
    }
}