using System.IO;
using System.Net;
using System.Threading;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AsyncRequestBodyTests
    {
        private const string BASE_URL = "http://localhost:8888/";

        [Test]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Have_No_Body_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);
                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method httpMethod = Method.PUT;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method httpMethod = Method.DELETE;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method httpMethod = Method.HEAD;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method httpMethod = Method.OPTIONS;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method httpMethod = Method.PATCH;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                client.ExecuteAsync(request, response => resetEvent.Set());
                resetEvent.WaitOne();

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        private static void AssertHasNoRequestBody()
        {
            Assert.Null(RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(false, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(string.Empty, RequestBodyCapturer.CapturedEntityBody);
        }

        private static void AssertHasRequestBody(string contentType, string bodyData)
        {
            Assert.AreEqual(contentType, RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(true, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(bodyData, RequestBodyCapturer.CapturedEntityBody);
        }

        private class RequestBodyCapturer
        {
            public const string RESOURCE = "Capture";

            public static string CapturedContentType { get; set; }

            public static bool CapturedHasEntityBody { get; set; }

            public static string CapturedEntityBody { get; set; }

            public static void Capture(HttpListenerContext context)
            {
                HttpListenerRequest request = context.Request;

                CapturedContentType = request.ContentType;
                CapturedHasEntityBody = request.HasEntityBody;
                CapturedEntityBody = StreamToString(request.InputStream);
            }

            private static string StreamToString(Stream stream)
            {
                StreamReader streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
