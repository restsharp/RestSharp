using System.Collections;

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

        var client = new RestClient(BaseUrl);
        client.AddDefaultHeaders(headers);

        var actual = client.DefaultParameters.Select(x => x as HeaderParameter);
        expected.Should().BeSubsetOf(actual);
    }

    [Fact]
    public void AddUrlSegmentWithInt() {
        const string name = "foo";

        var request  = new RestRequest().AddUrlSegment(name, 1);
        var actual   = request.Parameters.FirstOrDefault(x => x.Name == name);
        var expected = new UrlSegmentParameter(name, "1");
        
        expected.Should().BeEquivalentTo(actual);
    }
}