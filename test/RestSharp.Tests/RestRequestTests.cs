namespace RestSharp.Tests;

public class RestRequestTests {
    [Fact]
    public void RestRequest_Request_Property() {
        var request = new RestRequest("resource");
        request.Resource.Should().Be("resource");
    }

    [Fact]
    public void RestRequest_Test_Already_Encoded() {
        const string resource = "/api/get?query=Id%3d198&another=notencoded&novalue=";
        const string baseUrl  = "https://example.com";

        var request    = new RestRequest(resource);
        var parameters = request.Parameters.ToArray();

        request.Resource.Should().Be("/api/get");
        parameters.Length.Should().Be(3);

        var expected = new[] {
            new { Name = "query", Value   = "Id%3d198", Type   = ParameterType.QueryString, Encode = false },
            new { Name = "another", Value = "notencoded", Type = ParameterType.QueryString, Encode = false },
            new { Name = "novalue", Value = "", Type           = ParameterType.QueryString, Encode = false }
        };
        parameters.Should().BeEquivalentTo(expected, options => options.ExcludingMissingMembers());
        
        using var client = new RestClient(baseUrl);
        var       actual = client.BuildUri(request);
        actual.AbsoluteUri.Should().Be($"{baseUrl}{resource}");
    }

    [Fact]
    public async Task RestRequest_Fail_On_Exception() {
        var req = new RestRequest("nonexisting");

        using var client = new RestClient(new RestClientOptions("http://localhost:12345") { ThrowOnAnyError = true });
        await Assert.ThrowsAsync<HttpRequestException>(() => client.ExecuteAsync(req));
    }
}