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

        var expected = headers.Select(x => new Parameter(x.Key, x.Value, ParameterType.HttpHeader));

        var options = new RestClientOptions(BaseUrl);
        var client  = new RestClient(options);
        client.AddDefaultHeaders(headers);

        expected.Should().BeSubsetOf(client.DefaultParameters);
    }
}