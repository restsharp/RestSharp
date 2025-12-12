namespace RestSharp.Tests.Integrated; 

public sealed class ResourceStringParametersTests : IDisposable {
    readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task Should_keep_to_parameters_with_the_same_name() {
        const string parameters = "?priority=Low&priority=Medium";

        var url = "";
        _server
            .Given(Request.Create())
            .RespondWith(Response.Create().WithCallback(req => {
                url = req.Url;
                return new ResponseMessage();
            }));
        
        using var client  = new RestClient(_server.Url!);
        var request = new RestRequest(parameters);

        await client.GetAsync(request);

        var query = new Uri(url).Query;
        query.Should().Be(parameters);
    }
}