using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AsyncTests
    {
        [Test]
        public void Can_Perform_GET_Async()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");
            const string val = "Basic async test";

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue(val)))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");

                client.ExecuteAsync(request, (response, asyncHandle) =>
                                             {
                                                 Assert.NotNull(response.Content);
                                                 Assert.AreEqual(val, response.Content);
                                                 resetEvent.Set();
                                             });

                resetEvent.WaitOne();
            }
        }

        [Test]
        public void Can_Perform_GET_Async_Without_Async_Handle()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");
            const string val = "Basic async test";

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue(val)))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");

                client.ExecuteAsync(request, response =>
                                             {
                                                 Assert.NotNull(response.Content);
                                                 Assert.AreEqual(val, response.Content);
                                                 resetEvent.Set();
                                             });

                resetEvent.WaitOne();
            }
        }

        [Test]
        public async Task Can_Perform_GET_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";
            const string val = "Basic async task test";

            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");
                var result = await client.ExecuteTaskAsync(request);

                Assert.NotNull(result.Content);
                Assert.AreEqual(val, result.Content);
            }
        }

        [Test]
        public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler()
        {
            const string baseUrl = "http://localhost:8888/";
            const string exceptionMessage = "Thrown from OnBeforeDeserialization";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("success");

                request.OnBeforeDeserialization += r => { throw new Exception(exceptionMessage); };

                var response = await client.ExecuteTaskAsync<Response>(request);

                Assert.AreEqual(exceptionMessage, response.ErrorMessage);
                Assert.AreEqual(ResponseStatus.Error, response.ResponseStatus);
            }
        }

        [Test]
        public async Task Can_Perform_ExecuteGetTaskAsync_With_Response_Type()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("success");
                var response = await client.ExecuteTaskAsync<Response>(request);

                Assert.AreEqual("Works!", response.Data.Message);
            }
        }

        [Test]
        public async Task Can_Perform_GetTaskAsync_With_Response_Type()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("success");
                var response = await client.GetTaskAsync<Response>(request);

                Assert.AreEqual("Works!", response.Message);
            }
        }

#if !APPVEYOR
        [Test]
        public void Can_Cancel_GET_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";
            const string val = "Basic async task test";

            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("timeout");
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task<IRestResponse> task = client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Assert.True(task.IsCanceled);
            }
        }
#endif

        [Test]
        public void Can_Cancel_GET_TaskAsync_With_Response_Type()
        {
            const string baseUrl = "http://localhost:8888/";
            const string val = "Basic async task test";

            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("timeout");
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task<IRestResponse<Response>> task = client.ExecuteTaskAsync<Response>(request, cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Assert.True(task.IsCanceled);
            }
        }

        [Test]
        public async Task Handles_GET_Request_Errors_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("404");
                var response = await client.ExecuteTaskAsync(request);

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Test]
        public async Task Handles_GET_Request_Errors_TaskAsync_With_Response_Type()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("404");
                var response = await client.ExecuteTaskAsync<Response>(request);

                Assert.Null(response.Data);
            }
        }

        [Test]
        public async Task Can_Timeout_GET_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                IRestRequest request = new RestRequest("timeout", Method.GET).AddBody("Body_Content");

                // Half the value of ResponseHandler.Timeout
                request.Timeout = 500;

                var response = await client.ExecuteTaskAsync(request);

                Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
            }
        }

        [Test]
        public async Task Can_Timeout_PUT_TaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                IRestRequest request = new RestRequest("timeout", Method.PUT).AddBody("Body_Content");

                // Half the value of ResponseHandler.Timeout
                request.Timeout = 500;

                var response = await client.ExecuteTaskAsync(request);

                Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
            }
        }

        private static void UrlToStatusCodeHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }

        private class ResponseHandler
        {
            private void error(HttpListenerContext context)
            {
                context.Response.StatusCode = 400;
                context.Response.Headers.Add("Content-Type", "application/xml");
                context.Response.OutputStream.WriteStringUtf8(
                    @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>");
            }

            private void success(HttpListenerContext context)
            {
                context.Response.OutputStream.WriteStringUtf8(
                    @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>");
            }

            private void timeout(HttpListenerContext context)
            {
                Thread.Sleep(1000);
            }
        }

        private class Response
        {
            public string Message { get; set; }
        }
    }
}
