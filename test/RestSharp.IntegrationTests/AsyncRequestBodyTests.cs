using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AsyncRequestBodyTests
    {
        [OneTimeSetUp]
        public void Setup() => _server = SimpleServer.Create(Handlers.Generic<RequestBodyCapturer>());

        [OneTimeTearDown]
        public void Teardown() => _server.Dispose();

        [SetUp]
        public void CreateClient() => _client = new RestClient(_server.Url);

        SimpleServer _server;
        RestClient   _client;

        static void AssertHasNoRequestBody()
        {
            Assert.Null(RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(false, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(string.Empty, RequestBodyCapturer.CapturedEntityBody);
        }

        static void AssertHasRequestBody(string contentType, string bodyData)
        {
            Assert.AreEqual(contentType, RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(true, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(bodyData, RequestBodyCapturer.CapturedEntityBody);
        }

        class RequestBodyCapturer
        {
            public const string RESOURCE = "Capture";

            public static string CapturedContentType { get; set; }

            public static bool CapturedHasEntityBody { get; set; }

            public static string CapturedEntityBody { get; set; }

            public static void Capture(HttpListenerContext context)
            {
                var request = context.Request;

                CapturedContentType   = request.ContentType;
                CapturedHasEntityBody = request.HasEntityBody;
                CapturedEntityBody    = StreamToString(request.InputStream);
            }

            static string StreamToString(Stream stream)
            {
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }

        [Test]
        public void Can_Be_Added_To_COPY_Request()
        {
            const Method httpMethod = Method.COPY;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method httpMethod = Method.DELETE;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method httpMethod = Method.OPTIONS;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method httpMethod = Method.PATCH;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method httpMethod = Method.PUT;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Have_No_Body_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            var request    = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);
            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method httpMethod = Method.HEAD;

            var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            var resetEvent = new ManualResetEvent(false);

            _client.ExecuteAsync(request, response => resetEvent.Set());
            resetEvent.WaitOne();

            AssertHasNoRequestBody();
        }
    }
}