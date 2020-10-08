using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class MultipartFormDataTests
    {
        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(RequestHandler.Handle);
            _client = new RestClient(_server.Url);
        }

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        const string LineBreak = "\r\n";

        readonly string _expected =
            "--{0}"                                                       + LineBreak +
            "Content-Disposition: form-data; name=\"foo\""                + LineBreak + LineBreak +
            "bar"                                                         + LineBreak +
            "--{0}"                                                       + LineBreak +
            "Content-Disposition: form-data; name=\"a name with spaces\"" + LineBreak + LineBreak +
            "somedata"                                                    + LineBreak +
            "--{0}--"                                                     + LineBreak;

        readonly string _expectedFileAndBodyRequestContent =
            "--{0}"                                                                        + LineBreak +
            "Content-Type: application/json"                                               + LineBreak +
            "Content-Disposition: form-data; name=\"controlName\""                         + LineBreak + LineBreak +
            "test"                                                                         + LineBreak +
            "--{0}"                                                                        + LineBreak +
            "Content-Disposition: form-data; name=\"fileName\"; filename=\"TestFile.txt\"" + LineBreak +
            "Content-Type: application/octet-stream"                                       + LineBreak + LineBreak +
            "This is a test file for RestSharp."                                           + LineBreak +
            "--{0}--"                                                                      + LineBreak;

        readonly string _expectedDefaultMultipartContentType =
            "multipart/form-data; boundary={0}";

        readonly string _expectedCustomMultipartContentType =
            "multipart/vnd.resteasy+form-data; boundary={0}";

        SimpleServer _server;
        RestClient   _client;

        static class RequestHandler
        {
            public static string CapturedContentType { get; set; }

            public static void Handle(HttpListenerContext context)
            {
                CapturedContentType = context.Request.ContentType;
                Handlers.Echo(context);
            }
        }

        static void AddParameters(IRestRequest request)
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
                Method                  = Method.POST
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
                Method                  = Method.POST
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            IRestResponse syncResponse = null;

            using (var eventWaitHandle = new AutoResetEvent(false))
            {
                _client.ExecuteAsync(
                    request, response =>
                    {
                        syncResponse = response;
                        eventWaitHandle.Set();
                    }
                );

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
                Method                  = Method.POST
            };

            request.AddParameter("title", "test", ParameterType.RequestBody);

            var response = await _client.ExecuteAsync(request);
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

            string boundary = null;
            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            var response = _client.Execute(request);

            var expected = string.Format(_expected, boundary);

            Assert.AreEqual(expected, response.Content);
        }

        [Test]
        public void MultipartFormData_HasDefaultContentType()
        {
            var request = new RestRequest("/", Method.POST);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            string boundary = null;
            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            var response = _client.Execute(request);

            var expectedFileAndBodyRequestContent = string.Format(_expectedFileAndBodyRequestContent, boundary);
            var expectedDefaultMultipartContentType= string.Format(_expectedDefaultMultipartContentType, boundary);

            Assert.AreEqual(expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(expectedDefaultMultipartContentType, RequestHandler.CapturedContentType);
        }

        [Test]
        public void MultipartFormData_WithCustomContentType()
        {
            var request = new RestRequest("/", Method.POST);

            var path              = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
            var customContentType = "multipart/vnd.resteasy+form-data";
            request.AddHeader("Content-Type", customContentType);

            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            string boundary = null;
            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            var response = _client.Execute(request);

            var expectedFileAndBodyRequestContent = string.Format(_expectedFileAndBodyRequestContent, boundary);
            var expectedCustomMultipartContentType= string.Format(_expectedCustomMultipartContentType, boundary);

            Assert.AreEqual(expectedFileAndBodyRequestContent, response.Content);
            Assert.AreEqual(expectedCustomMultipartContentType, RequestHandler.CapturedContentType);
        }

        [Test]
        public void MultipartFormData_WithParameterAndFile()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            string boundary = null;
            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            var response = _client.Execute(request);

            var expectedFileAndBodyRequestContent = string.Format(_expectedFileAndBodyRequestContent, boundary);

            Assert.AreEqual(expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public async Task MultipartFormData_WithParameterAndFile_Async()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
            request.AddFile("fileName", path);

            request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

            string boundary = null;
            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            var response = await _client.ExecuteAsync(request);

            var expectedFileAndBodyRequestContent = string.Format(_expectedFileAndBodyRequestContent, boundary);

            Assert.AreEqual(expectedFileAndBodyRequestContent, response.Content);
        }

        [Test]
        public void MultipartFormDataAsync()
        {
            var request = new RestRequest("/", Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            AddParameters(request);

            string boundary = null;

            var expected = string.Format(_expected, boundary);

            request.OnBeforeRequest += http => boundary = http.FormBoundary;

            _client.ExecuteAsync(
                request, (restResponse, handle) =>
                {
                    Console.WriteLine(restResponse.Content);
                    Assert.AreEqual(expected, restResponse.Content);
                }
            );
        }
    }
}