namespace RestSharp.Tests.Integrated;

public sealed class NonProtocolExceptionHandlingTests : IDisposable {
    public NonProtocolExceptionHandlingTests()
        => _server
            .Given(Request.Create().WithPath("/timeout"))
            .RespondWith(Response.Create().WithDelay(TimeSpan.FromSeconds(1)));

    // ReSharper disable once ClassNeverInstantiated.Local
    class StupidClass {
        // ReSharper disable once UnusedMember.Local
        public string Property { get; set; } = null!;
    }

    public void Dispose() => _server.Dispose();

    readonly WireMockServer _server = WireMockServer.Start();

    /// <summary>
    /// Success of this test is based largely on the behavior of your current DNS.
    /// For example, if you're using OpenDNS this will test will fail; ResponseStatus will be Completed.
    /// </summary>
    [Fact]
    public async Task Handles_Non_Existent_Domain() {
        using var client = new RestClient("http://nonexistantdomainimguessing.org");

        var request  = new RestRequest("foo");
        var response = await client.ExecuteAsync(request);

        response.ResponseStatus.Should().Be(ResponseStatus.Error);
    }

    [Fact]
    public async Task Handles_HttpClient_Timeout_Error() {
        using var client = new RestClient(new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) });

        var request  = new RestRequest($"{_server.Url}/timeout");
        var response = await client.ExecuteAsync(request);

        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Handles_Server_Timeout_Error() {
        using var client = new RestClient(_server.Url!);

        var request  = new RestRequest("timeout") { Timeout = TimeSpan.FromMilliseconds(500) };
        var response = await client.ExecuteAsync(request);

        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Handles_Server_Timeout_Error_With_Deserializer() {
        using var client = new RestClient(_server.Url!);

        var request  = new RestRequest("timeout") { Timeout = TimeSpan.FromMilliseconds(500) };
        var response = await client.ExecuteAsync<SuccessResponse>(request);

        response.Data.Should().BeNull();
        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Task_Handles_Non_Existent_Domain() {
        using var client = new RestClient("http://this.cannot.exist:8001");

        var request = new RestRequest("/") {
            RequestFormat = DataFormat.Json,
            Method        = Method.Get
        };
        var response = await client.ExecuteAsync<StupidClass>(request);

        response.ErrorException.Should().BeOfType<HttpRequestException>();
        response.ErrorException!.Message.Should().Contain("known");
        response.ResponseStatus.Should().Be(ResponseStatus.Error);
    }
}