using System.Collections;
using System.IO;

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

    [Fact]
    public void AddUrlSegmentModifiesUrlSegmentWithInt() {
        const string name = "foo";
        var path = $"/{{{name}}}/resource";
        var urlSegmentValue = 1;

        var request = new RestRequest(path).AddUrlSegment(name, urlSegmentValue);

        var expected = GetFormattedUrlSegmentString(path, request.Parameters);

        var client = new RestClient(BaseUrl);

        var actual = client.BuildUri(request).AbsolutePath;
        

        expected.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void AddUrlSegmentModifiesUrlSegmentWithString() {
        const string name = "foo";
        var path = $"/{{{name}}}/resource";
        var urlSegmentValue = "bar";

        var request = new RestRequest(path).AddUrlSegment(name, urlSegmentValue);

        var expected = GetFormattedUrlSegmentString(path, request.Parameters);

        var client = new RestClient(BaseUrl);

        var actual = client.BuildUri(request).AbsolutePath;

        expected.Should().BeEquivalentTo(actual);

    }

    private string GetFormattedUrlSegmentString(string str, ParametersCollection parametersCollection) {
        var parameters = parametersCollection
            .Where(x => x.Type == ParameterType.UrlSegment)
            .ToDictionary(x => "{" + x.Name + "}", x => x.Value);

        return parameters.Aggregate(str, (current, parameter) => current.Replace(parameter.Key, parameter.Value.ToString()));
    }
}