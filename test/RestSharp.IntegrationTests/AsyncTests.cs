using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AsyncTests
    {
        static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());

        class ResponseHandler
        {
            void error(HttpListenerContext context)
            {
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

        class Response
        {
            public string Message { get; set; }
        }

        [Test]
        public void Can_Cancel_GET_TaskAsync_With_Response_Type()
        {
            const string val = "Basic async task test";

            using var server = SimpleServer.Create(Handlers.EchoValue(val));

            var client                  = new RestClient(server.Url);
            var request                 = new RestRequest("timeout");
            var cancellationTokenSource = new CancellationTokenSource();
            var task                    = client.ExecuteAsync<Response>(request, cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            Assert.True(task.IsCanceled);
        }

        [Test]
        public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler()
        {
            const string exceptionMessage = "Thrown from OnBeforeDeserialization";

            using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

            var client  = new RestClient(server.Url);
            var request = new RestRequest("success");

            request.OnBeforeDeserialization += r => throw new Exception(exceptionMessage);

            var response = await client.ExecuteAsync<Response>(request);

            Assert.AreEqual(exceptionMessage, response.ErrorMessage);
            Assert.AreEqual(ResponseStatus.Error, response.ResponseStatus);
        }

        [Test]
        public async Task Can_Perform_ExecuteGetTaskAsync_With_Response_Type()
        {
            using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("success");
            var response = await client.ExecuteAsync<Response>(request);

            Assert.AreEqual("Works!", response.Data.Message);
        }

        [Test]
        public void Can_Perform_GET_Async()
        {
            const string val = "Basic async test";

            var resetEvent = new ManualResetEvent(false);

            using var server = SimpleServer.Create(Handlers.EchoValue(val));

            var client  = new RestClient(server.Url);
            var request = new RestRequest("");

            client.ExecuteAsync(
                request, (response, asyncHandle) =>
                {
                    Assert.NotNull(response.Content);
                    Assert.AreEqual(val, response.Content);
                    resetEvent.Set();
                }
            );

            resetEvent.WaitOne();
        }

        [Test]
        public void Can_Perform_GET_Async_Without_Async_Handle()
        {
            const string val = "Basic async test";

            var resetEvent = new ManualResetEvent(false);

            using var server = SimpleServer.Create(Handlers.EchoValue(val));

            var client  = new RestClient(server.Url);
            var request = new RestRequest("");

            client.ExecuteAsync(
                request, response =>
                {
                    Assert.NotNull(response.Content);
                    Assert.AreEqual(val, response.Content);
                    resetEvent.Set();
                }
            );

            resetEvent.WaitOne();
        }

        [Test]
        public async Task Can_Perform_GET_TaskAsync()
        {
            const string val = "Basic async task test";

            using var server = SimpleServer.Create(Handlers.EchoValue(val));

            var client  = new RestClient(server.Url);
            var request = new RestRequest("");
            var result  = await client.ExecuteAsync(request);

            Assert.NotNull(result.Content);
            Assert.AreEqual(val, result.Content);
        }

        [Test]
        public async Task Can_Perform_GetTaskAsync_With_Response_Type()
        {
            using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("success");
            var response = await client.GetAsync<Response>(request);

            Assert.AreEqual("Works!", response.Message);
        }

        [Test]
        public async Task Can_Timeout_GET_TaskAsync()
        {
            using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

            var client  = new RestClient(server.Url);
            var request = new RestRequest("timeout", Method.GET).AddBody("Body_Content");

            // Half the value of ResponseHandler.Timeout
            request.Timeout = 500;

            var response = await client.ExecuteAsync(request);

            Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
        }

        [Test]
        public async Task Can_Timeout_PUT_TaskAsync()
        {
            using var server = SimpleServer.Create(Handlers.Generic<ResponseHandler>());

            var client  = new RestClient(server.Url);
            var request = new RestRequest("timeout", Method.PUT).AddBody("Body_Content");

            // Half the value of ResponseHandler.Timeout
            request.Timeout = 500;

            var response = await client.ExecuteAsync(request);

            Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
        }

        [Test]
        public async Task Handles_GET_Request_Errors_TaskAsync()
        {
            using var server = SimpleServer.Create(UrlToStatusCodeHandler);

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("404");
            var response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Handles_GET_Request_Errors_TaskAsync_With_Response_Type()
        {
            using var server = SimpleServer.Create(UrlToStatusCodeHandler);

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("404");
            var response = await client.ExecuteAsync<Response>(request);

            Assert.Null(response.Data);
        }
    }
}