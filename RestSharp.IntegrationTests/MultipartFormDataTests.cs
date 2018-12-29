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
        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(BaseUrl, RequestHandler.Handle);
            _client = new RestClient(BaseUrl);
        }

        [TearDown]
        public void ShutdownServer()
        {
            _server.Dispose();
        }

        private const string LineBreak = "\r\n";

        private readonly string _expected =
            "-------------------------------28947758029299" + LineBreak +
            "Content-Disposition: form-data; name=\"foo\"" + LineBreak + LineBreak +
            "bar" + LineBreak +
            "-------------------------------28947758029299" + LineBreak +
            "Content-Disposition: form-data; name=\"a name with spaces\"" + LineBreak + LineBreak +
            "somedata" + LineBreak +
            "-------------------------------28947758029299--" + LineBreak;

        private readonly string _expectedFileAndBodyRequestContent =
            "-------------------------------28947758029299" + LineBreak +
            "Content-Type: application/json" + LineBreak +
            "Content-Disposition: form-data; name=\"controlName\"" + LineBreak + LineBreak +
            "test" + LineBreak +
            "-------------------------------28947758029299" + LineBreak +
            "Content-Disposition: form-data; name=\"fileName\"; filename=\"TestFile.txt\"" + LineBreak +
            "Content-Type: application/octet-stream" + LineBreak + LineBreak +
            "This is a test file for RestSharp." + LineBreak +
            "-------------------------------28947758029299--" + LineBreak;

        private readonly string _expectedDefaultMultipartContentType =
            "multipart/form-data; boundary=-----------------------------28947758029299";

        private readonly string _expectedCustomMultipartContentType =
            "multipart/vnd.resteasy+form-data; boundary=-----------------------------28947758029299";

        private SimpleServer _server;
        private RestClient _client;

        private const string BaseUrl = "http://localhost:8888/";

        private static class RequestHandler
        {
            public static string CapturedContentType { get; set; }

            public static void Handle(HttpListenerContext context)
            {
                CapturedContentType = context.Request.ContentType;
                Handlers.Echo(context);
            }
        }

        private static void AddParameters(IRestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_Execute()
        {
            var request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            var response = _client.Execute(request);

            Assert.Null(response.ErrorException);
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_ExecuteAsync()
        {
            var request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            IRestResponse syncResponse = null;

            using (var eventWaitHandle = new AutoResetEvent(false))
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
        public async Task AlwaysMultipartFormData_WithParameter_ExecuteTaskAsync()
        {
            var request = new RestRequest("?json_route=/posts")
            {
                AlwaysMultipartFormData = true,
                Method = Method.POST
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            var response = await _client.ExecuteTaskAsync(request);
            Assert.Null(response.ErrorException);
        }

        [Test]
        public void MultipartFormData()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            AddParameters(request);

            var response = _client.Execute(request);

            Assert.AreEqual(_expected, response.Content);
        }

        [Test]
        public void MultipartFormData_HasDefaultContentType()
        {
            var request = new RestRequest("/", Method.POST);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            var response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(_expectedDefaultMultipartContentType, RequestHandler.CapturedContentType);
        }

        [Test]
        public void MultipartFormData_WithCustomContentType()
        {
            var request = new RestRequest("/", Method.POST);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            var customContentType = "multipart/vnd.resteasy+form-data";
            request.AddHeader("Content-Type", customContentType);

            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            var response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(_expectedCustomMultipartContentType, RequestHandler.CapturedContentType);
        }

        [Test]
        public void MultipartFormData_WithParameterAndFile()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            var response = _client.Execute(request);

            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public async Task MultipartFormData_WithParameterAndFile_Async()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            var response = await _client.ExecuteTaskAsync(request);
            Assert.AreEqual(_expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public void MultipartFormDataAsync()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            AddParameters(request);

            _client.ExecuteAsync(request, (restResponse, handle) =>
            {
                Console.WriteLine(restResponse.Content);
                Assert.AreEqual(_expected, restResponse.Content);
            });
        }
    }
}