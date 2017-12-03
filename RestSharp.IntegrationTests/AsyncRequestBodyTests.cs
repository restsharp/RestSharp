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
        private SimpleServer _server;
        private RestClient _client;
        private const string BaseUrl = "http://localhost:8888/";

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        [SetUp]
        public void CreateClient()
        {
            _server = SimpleServer.Create(BaseUrl, Handlers.Generic<RequestBodyCapturer>());
            _client = new RestClient(BaseUrl);
        }

        [Test]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Have_No_Body_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method httpMethod = Method.PUT;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method httpMethod = Method.DELETE;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method httpMethod = Method.HEAD;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method httpMethod = Method.OPTIONS;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method httpMethod = Method.PATCH;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_COPY_Request()
        {
            const Method httpMethod = Method.COPY;

            RestRequest request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
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
            public const string Resource = "Capture";

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