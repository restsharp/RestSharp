using System.Net;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class AsyncTests {
    static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());

    class ResponseHandler {
        void error(HttpListenerContext context) {
            context.Response.StatusCode = 400;
            context.Response.Headers.Add("Content-Type", "application/xml");

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
    public void Can_Cancel_GET_Async_With_Response_Type() {
        const string val = "Basic async task test";

        using var server = SimpleServer.Create(Handlers.EchoValue(val));

        var client                  = new RestClient(server.Url);
        var request                 = new RestRequest("timeout");
        var cancellationTokenSource = new CancellationTokenSource();
        var task                    = client.ExecuteAsync<Response>(request, cancellationTokenSource.Token);

        cancellationTokenSource.Cancel();

        Assert.True(task.IsCanceled);
    }

    [Fact]
    public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client  = new RestClient(server.Url);
        var request = new RestRequest("success");

        request.OnBeforeDeserialization += r => throw new Exception(exceptionMessage);

        var response = await client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_ExecuteGetAsync_With_Response_Type() {
        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("success");
        var response = await client.ExecuteAsync<Response>(request);

        Assert.Equal("Works!", response.Data.Message);
    }

    [Fact]
    public async Task Can_Perform_GET_Async() {
        const string val = "Basic async test";

        using var server = SimpleServer.Create(Handlers.EchoValue(val));

        var client  = new RestClient(server.Url);
        var request = new RestRequest("");

        var response = await client.ExecuteAsync(request);
        response.Content.Should().Be(val);
    }

    [Fact]
    public async Task Can_Perform_GetTaskAsync_With_Response_Type() {
        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("success");
        var response = await client.GetAsync<Response>(request);

        Assert.Equal("Works!", response.Message);
    }

    [Fact]
    public async Task Can_Timeout_GET_TaskAsync() {
        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client  = new RestClient(server.Url);
        var request = new RestRequest("timeout", Method.Get).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 500;

        var response = await client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Timeout_PUT_TaskAsync() {
        using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

        var client  = new RestClient(server.Url);
        var request = new RestRequest("timeout", Method.Put).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 500;

        var response = await client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Handles_GET_Request_Errors_TaskAsync() {
        using var server = SimpleServer.Create(UrlToStatusCodeHandler);

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("404");
        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handles_GET_Request_Errors_TaskAsync_With_Response_Type() {
        using var server = SimpleServer.Create(UrlToStatusCodeHandler);

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("404");
        var response = await client.ExecuteAsync<Response>(request);

        Assert.Null(response.Data);
    }
}