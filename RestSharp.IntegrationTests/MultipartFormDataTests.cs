using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class MultipartFormDataTests
    {
        private readonly string _expected =
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"foo\"" + Environment.NewLine + Environment.NewLine +
            "bar" + Environment.NewLine +
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"a name with spaces\"" + Environment.NewLine + Environment.NewLine +
            "somedata" + Environment.NewLine +
            "-------------------------------28947758029299--" + Environment.NewLine;

        private readonly string _expectedFileAndBodyRequestContent =
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Type: application/json" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"controlName\"" + Environment.NewLine + Environment.NewLine +
            "test" + Environment.NewLine +
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"fileName\"; filename=\"TestFile.txt\"" + Environment.NewLine +
            "Content-Type: application/octet-stream" + Environment.NewLine + Environment.NewLine +
            "This is a test file for RestSharp." + Environment.NewLine +
            "-------------------------------28947758029299--" + Environment.NewLine;

        private readonly string _expectedDefaultMultipartContentType = 
            "multipart/form-data; boundary=-----------------------------28947758029299";

        private readonly string _expectedCustomMultipartContentType = 
            "multipart/vnd.resteasy+form-data; boundary=-----------------------------28947758029299";

        private SimpleServer _server;
        private RestClient _client;

        private const string BaseUrl = "http://localhost:8888/";

        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(BaseUrl, RequestHandler.Handle);
            _client = new RestClient(BaseUrl);
        }

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        [Test]
        public async Task MultipartFormData_WithParameterAndFile_Async()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            var response = await _client.ExecuteTaskAsync(request);
            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public void MultipartFormData_WithParameterAndFile()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public void MultipartFormDataAsync()
        {
            RestRequest request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            AddParameters(request);

            _client.ExecuteAsync(request, (restResponse, handle) =>
            {
                Console.WriteLine(restResponse.Content);
                Assert.AreEqual(this._expected, restResponse.Content);
            });
        }

        [Test]
        public void MultipartFormData()
        {
            RestRequest request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            AddParameters(request);

            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(_expected, response.Content);
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_Execute()
        {
            RestRequest request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST,
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            Assert.Null(response.ErrorException);
        }

        [Test]
        public async Task AlwaysMultipartFormData_WithParameter_ExecuteTaskAsync()
        {
            var request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST,
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            var response = await _client.ExecuteTaskAsync(request);
            Assert.Null(response.ErrorException);
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_ExecuteAsync()
        {
            RestRequest request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST,
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            IRestResponse syncResponse = null;

            using (AutoResetEvent eventWaitHandle = new AutoResetEvent(false))
            {
                _client.ExecuteAsync(request, response =>
                {
                    syncResponse = response;
                    eventWaitHandle.Set();
                });

                eventWaitHandle.WaitOne();
            }

            Assert.Null(syncResponse.ErrorException);
        }

        [Test]
        public void MultipartFormData_HasDefaultContentType() {
            var request = new RestRequest("/", Method.POST);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(_expectedDefaultMultipartContentType, RequestHandler.CapturedContentType);
        }

        [Test]
        public void MultipartFormData_WithCustomContentType() {
            var request = new RestRequest("/", Method.POST);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            string customContentType = "multipart/vnd.resteasy+form-data";
            request.AddHeader("Content-Type", customContentType);

            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(_expectedCustomMultipartContentType, RequestHandler.CapturedContentType);
        }

        private static class RequestHandler {
            public static string CapturedContentType { get; set; }

            public static void Handle(HttpListenerContext context) {
                CapturedContentType = context.Request.ContentType;
                Handlers.Echo(context);
            }
        }

        private static void AddParameters(IRestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }
    }
}