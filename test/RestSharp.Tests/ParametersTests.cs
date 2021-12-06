namespace RestSharp.Tests; 

public class ParametersTests {
    const string BaseUrl = "http://localhost:8888/";

    [Fact]
    public void AddDefaultHeadersUsingDictionary() {
        var headers = new Dictionary<string, string> {
            { "Content-Type", "application/json" },
            { "Accept", "application/json" },
            { "Content-Encoding", "gzip, deflate" }
        };

        var expected = headers.Select(x => new Parameter(x.Key, x.Value, ParameterType.HttpHeader));

        var client = new RestClient(BaseUrl);
        client.AddDefaultHeaders(headers);

        expected.Should().BeSubsetOf(client.DefaultParameters);
    }
}