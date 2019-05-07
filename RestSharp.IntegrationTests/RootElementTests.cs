using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using RestSharp.IntegrationTests.SampleDeserializers;
using RestSharp.Serialization;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class RootElementTests
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
        public void Copy_RootElement_From_Request_To_IWithRootElement_Deserializer()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());
            RestRequest request = new RestRequest("success")
            {
                RootElement = "Success"
            };
            var deserializer = new CustomDeserializer();
            _client.AddHandler(ContentType.Xml, () => deserializer);
            _client.Execute<Response>(request);

            Assert.AreEqual(deserializer.RootElement, request.RootElement);
        }

        public class ResponseHandler
        {

            private void success(HttpListenerContext context)
            {
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("Content-Type", ContentType.Xml);
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

        private static void UrlToStatusCodeHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }
    }
}