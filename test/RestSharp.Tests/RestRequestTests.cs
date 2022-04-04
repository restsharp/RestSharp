namespace RestSharp.Tests;

public class RestRequestTests {
    [Fact]
    public void RestRequest_Request_Property() {
        var request = new RestRequest("resource");

        Assert.Equal("resource", request.Resource);
    }

    [Fact]
    public void RestRequest_Test_Already_Encoded() {
        var request    = new RestRequest("/api/get?query=Id%3d198&another=notencoded");
        var parameters = request.Parameters.ToArray();

        Assert.Equal("/api/get", request.Resource);
        Assert.Equal(2, request.Parameters.Count);
        Assert.Equal("query", parameters[0].Name);
        Assert.Equal("Id%3d198", parameters[0].Value);
        Assert.Equal(ParameterType.QueryString, parameters[0].Type);
        Assert.False(parameters[0].Encode);
        Assert.Equal("another", parameters[1].Name);
        Assert.Equal("notencoded", parameters[1].Value);
        Assert.Equal(ParameterType.QueryString, parameters[1].Type);
        Assert.False(parameters[1].Encode);
    }

    [Fact]
    public async Task RestRequest_Fail_On_Exception() {
        var req    = new RestRequest("nonexisting");
        var client = new RestClient(new RestClientOptions("http://localhost:12345") { ThrowOnAnyError = true });
        await Assert.ThrowsAsync<HttpRequestException>(() => client.ExecuteAsync(req));
    }
}