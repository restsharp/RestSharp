using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.Serialization;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StatusCodeTests
    {
        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(UrlToStatusCodeHandler);
            _client = new RestClient(_server.Url);
        }

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        SimpleServer _server;
        RestClient   _client;

        static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());

        [Test]
        public void ContentType_Additional_Information()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());

            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                Resource      = "contenttype_odata"
            };
            request.AddBody("bodyadsodajjd");
            request.AddHeader("X-RequestDigest", "xrequestdigestasdasd");
            request.AddHeader("Accept", $"{ContentType.Json}; odata=verbose");
            request.AddHeader("Content-Type", $"{ContentType.Json}; odata=verbose");

            var response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void Handles_Default_Root_Element_On_No_Error()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());

            var request = new RestRequest("success")
            {
                RootElement = "Success"
            };

            request.OnBeforeDeserialization = resp =>
            {
                if (resp.StatusCode == HttpStatusCode.NotFound) request.RootElement = "Error";
            };

            var response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Works!", response.Data.Message);
        }

        [Test]
        public void Handles_Different_Root_Element_On_Http_Error()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());

            var request = new RestRequest("error")
            {
                RootElement = "Success"
            };

            request.OnBeforeDeserialization =
                resp =>
                {
                    if (resp.StatusCode == HttpStatusCode.BadRequest) request.RootElement = "Error";
                };

            var response = _client.Execute<Response>(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("Not found!", response.Data.Message);
        }

        [Test]
        public void Handles_GET_Request_404_Error()
        {
            var request  = new RestRequest("404");
            var response = _client.Execute(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test, Ignore("Not sure why this hangs")]
        public void Reports_1xx_Status_Code_Success_Accurately()
        {
            var request  = new RestRequest("100");
            var response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_2xx_Status_Code_Success_Accurately()
        {
            var request  = new RestRequest("204");
            var response = _client.Execute(request);

            Assert.IsTrue(response.IsSuccessful);
        }

        [Test]
        public void Reports_3xx_Status_Code_Success_Accurately()
        {
            var request  = new RestRequest("301");
            var response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_4xx_Status_Code_Success_Accurately()
        {
            var request  = new RestRequest("404");
            var response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }

        [Test]
        public void Reports_5xx_Status_Code_Success_Accurately()
        {
            var request  = new RestRequest("503");
            var response = _client.Execute(request);

            Assert.IsFalse(response.IsSuccessful);
        }
    }

    public class ResponseHandler
    {
        void contenttype_odata(HttpListenerContext context)
        {
            var hasCorrectHeader = context.Request.Headers["Content-Type"] == $"{ContentType.Json}; odata=verbose";
            context.Response.StatusCode = hasCorrectHeader ? 200 : 400;
        }

        void error(HttpListenerContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.Headers.Add("Content-Type", ContentType.Xml);

            context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>"
            );
        }

        void errorwithbody(HttpListenerContext context)
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
    }

    public class Response
    {
        public string Message { get; set; }
    }
}