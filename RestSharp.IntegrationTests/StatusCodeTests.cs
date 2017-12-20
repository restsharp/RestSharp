using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StatusCodeTests
    {
        private readonly Uri _baseUrl = new Uri("http://localhost:8888/");
        private SimpleServer _server;
        private RestClient _client;

        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(_baseUrl.AbsoluteUri, UrlToStatusCodeHandler);
            _client = new RestClient(_baseUrl);
        }

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        [Test]
        public void Handles_GET_Request_404_Error()
        {
            RestRequest request = new RestRequest("404");
            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void Handles_GET_Request_404_Error_With_Body()
        {
            RestRequest request = new RestRequest("404");

            request.AddBody("This is the body");

            IRestResponse response = _client.Execute(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static void UrlToStatusCodeHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }

        [Test]
        public void Handles_Different_Root_Element_On_Http_Error()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());
            RestRequest request = new RestRequest("error")
            {
                RootElement = "Success"
            };

            request.OnBeforeDeserialization =
                resp =>
                {
                    if (resp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        request.RootElement = "Error";
                    }
                };

            IRestResponse<Response> response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("Not found!", response.Data.Message);
        }

        [Test]
        public void Handles_Default_Root_Element_On_No_Error()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());
            RestRequest request = new RestRequest("success")
            {
                RootElement = "Success"
            };

            request.OnBeforeDeserialization = resp =>
            {
                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    request.RootElement = "Error";
                }
            };

            IRestResponse<Response> response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Works!", response.Data.Message);
        }

        [Test]
        [Ignore("Not sure why this hangs")]
        public void Reports_1xx_Status_Code_Success_Accurately()
        {
            RestRequest request = new RestRequest("100");
            IRestResponse response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_2xx_Status_Code_Success_Accurately()
        {
            RestRequest request = new RestRequest("204");
            IRestResponse response = _client.Execute(request);

            Assert.IsTrue(response.IsSuccessful);
        }

        [Test]
        public void Reports_3xx_Status_Code_Success_Accurately()
        {
            RestRequest request = new RestRequest("301");
            IRestResponse response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_4xx_Status_Code_Success_Accurately()
        {
            RestRequest request = new RestRequest("404");
            IRestResponse response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_5xx_Status_Code_Success_Accurately()
        {
            RestRequest request = new RestRequest("503");
            IRestResponse response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void ContentType_Additional_Information()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());
            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                Resource = "contenttype_odata"
            };
            request.AddBody("bodyadsodajjd");
            request.AddHeader("X-RequestDigest", "xrequestdigestasdasd");
            request.AddHeader("Accept", "application/json; odata=verbose");
            request.AddHeader("Content-Type", "application/json; odata=verbose");

            IRestResponse<Response> response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class ResponseHandler
    {
        private void contenttype_odata(HttpListenerContext context)
        {
            bool hasCorrectHeader = context.Request.Headers["Content-Type"] == "application/json; odata=verbose";
            context.Response.StatusCode = hasCorrectHeader ? 200 : 400;
        }

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

        private void errorwithbody(HttpListenerContext context)
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
    }

    public class Response
    {
        public string Message { get; set; }
    }
}