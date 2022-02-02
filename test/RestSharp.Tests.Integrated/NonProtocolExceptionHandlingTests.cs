using System.Net;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class NonProtocolExceptionHandlingTests : IDisposable {
    // ReSharper disable once ClassNeverInstantiated.Local
    class StupidClass {
        public string Property { get; set; }
    }

    /// <summary>
    /// Simulates a long server process that should result in a client timeout
    /// </summary>
    /// <param name="context"></param>
    static void TimeoutHandler(HttpListenerContext context) => Thread.Sleep(101000);

    public NonProtocolExceptionHandlingTests() => _server = SimpleServer.Create(TimeoutHandler);

    public void Dispose() => _server.Dispose();

    readonly SimpleServer _server;

    /// <summary>
    /// Success of this test is based largely on the behavior of your current DNS.
    /// For example, if you're using OpenDNS this will test will fail; ResponseStatus will be Completed.
    /// </summary>
    [Fact]
    public async Task Handles_Non_Existent_Domain() {
        var client   = new RestClient("http://nonexistantdomainimguessing.org");
        var request  = new RestRequest("foo");
        var response = await client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Handles_HttpClient_Timeout_Error() {
        var client = new RestClient(new HttpClient {Timeout = TimeSpan.FromMilliseconds(500)});

        var request = new RestRequest($"{_server.Url}/404");
        var response = await client.ExecuteAsync(request);

        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Handles_Server_Timeout_Error() {
        var client = new RestClient(_server.Url);

        var request = new RestRequest("404") { Timeout = 500 };
        var response = await client.ExecuteAsync(request);

        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Handles_Server_Timeout_Error_With_Deserializer() {
        var client   = new RestClient(_server.Url);
        var request  = new RestRequest("404") { Timeout = 500 };
        var response = await client.ExecuteAsync<TestResponse>(request);

        response.Data.Should().BeNull();
        response.ErrorException.Should().BeOfType<TaskCanceledException>();
        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut);
    }

    [Fact]
    public async Task Task_Handles_Non_Existent_Domain() {
        var client = new RestClient("http://this.cannot.exist:8001");

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