using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class RequestBodyTests
    {
        SimpleServer _server;
        const string NewLine = "\r\n";

        [OneTimeSetUp]
        public void Setup() => _server = SimpleServer.Create(Handlers.Generic<RequestBodyCapturer>());

        [OneTimeTearDown]
        public void Teardown() => _server.Dispose();

        [Test]
        public void Can_Be_Added_To_COPY_Request()
        {
            const Method httpMethod = Method.COPY;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method httpMethod = Method.DELETE;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method httpMethod = Method.OPTIONS;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method httpMethod = Method.PATCH;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method httpMethod = Method.PUT;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasRequestBody(contentType, bodyData);
        }

        [Test]
        public void Can_Have_No_Body_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            client.Execute(request);

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasNoRequestBody();
        }

        [Test]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method httpMethod = Method.HEAD;

            var client  = new RestClient(_server.Url);
            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

            client.Execute(request);

            AssertHasNoRequestBody();
        }

        [Test]
        public void MultipartFormData_Without_File_Creates_A_Valid_RequestBody()
        {
            string expectedFormBoundary = null;

            var client = new RestClient(_server.Url);

            var request = new RestRequest(RequestBodyCapturer.Resource, Method.POST)
            {
                AlwaysMultipartFormData = true
            };
            request.OnBeforeRequest += http => expectedFormBoundary = http.FormBoundary;

            const string contentType   = "text/plain";
            const string bodyData      = "abc123 foo bar baz BING!";
            const string multipartName = "mybody";

            request.AddParameter(multipartName, bodyData, contentType, ParameterType.RequestBody);

            client.Execute(request);

            var expectedBody = "--" + expectedFormBoundary +
                NewLine
                + "Content-Type: " +
                contentType
                + NewLine
                + @"Content-Disposition: form-data; name=""" + multipartName + @""""
                + NewLine
                + NewLine
                + bodyData
                + NewLine
                + "--" + expectedFormBoundary + "--"
                + NewLine;

            Assert.AreEqual(
                expectedBody, RequestBodyCapturer.CapturedEntityBody, "Empty multipart generated: " + RequestBodyCapturer.CapturedEntityBody
            );
        }

        [Test]
        public void Query_Parameters_With_Json_Body()
        {
            const Method httpMethod = Method.PUT;

            var client = new RestClient(_server.Url);

            var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod)
                .AddJsonBody(new {displayName = "Display Name"})
                .AddQueryParameter("key", "value");

            client.Execute(request);

            Assert.AreEqual($"{_server.Url}Capture?key=value", RequestBodyCapturer.CapturedUrl.ToString());
            Assert.AreEqual("application/json", RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual("{\"displayName\":\"Display Name\"}", RequestBodyCapturer.CapturedEntityBody);
        }

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
    }
}