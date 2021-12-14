using System.Net;
using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class AsyncTests : IAsyncLifetime {
    readonly ITestOutputHelper _output;
    readonly HttpServer        _server;

    public AsyncTests(ITestOutputHelper output) {
        _output = output;
        _server = new HttpServer(output);
    }

    class ResponseHandler {
        void error(HttpListenerContext context) {
            context.Response.StatusCode = 400;
            context.Response.Headers.Add(KnownHeaders.ContentType, "application/xml");

            context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>"
            );
        }

        void success(HttpListenerContext context)
            => context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>"
            );

        void timeout(HttpListenerContext context) => Thread.Sleep(1000);
    }

    class Response {
        public string Message { get; set; }
    }

    [Fact]
    public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var client  = new RestClient(_server.Url);
        var request = new RestRequest("success");

        request.OnBeforeDeserialization += r => throw new Exception(exceptionMessage);

        var response = await client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_ExecuteGetAsync_With_Response_Type() {
        var client   = new RestClient(_server.Url);
        var request  = new RestRequest("success");
        var response = await client.ExecuteAsync<Response>(request);

        response.StatusCode.Should().Be(200);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Perform_GET_Async() {
        const string val = "Basic async test";

        var client  = new RestClient(_server.Url);
        var request = new RestRequest($"echo?msg={val}");

        var response = await client.ExecuteAsync(request);
        response.Content.Should().Be(val);
    }

    [Fact]
    public async Task Can_Timeout_GET_Async() {
        var client  = new RestClient(_server.Url);
        var request = new RestRequest("timeout", Method.Get).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 500;

        var response = await client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Timeout_PUT_Async() {
        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client  = new RestClient(server.Url);
        var request = new RestRequest("timeout", Method.Put).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 500;

        var response = await client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Handles_GET_Request_Errors_Async() {
        var client   = new RestClient(_server.Url);
        var request  = new RestRequest("status?code=404");
        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handles_GET_Request_Errors_Async_With_Response_Type() {
        var client   = new RestClient(_server.Url);
        var request  = new RestRequest("status?code=404");
        var response = await client.ExecuteAsync<Response>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Data);
    }

    public Task InitializeAsync() => _server.Start();

    public Task DisposeAsync() => _server.Stop();
}