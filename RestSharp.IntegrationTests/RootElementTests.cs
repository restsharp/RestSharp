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

        public class ResponseHandler
        {
            void Success(HttpListenerContext context)
            {
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("Content-Type", ContentType.Xml);

                context.Response.OutputStream.WriteStringUtf8(
                    @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>"
                );
            }
        }

        public class Response
        {
            public string Message { get; set; }
        }

        static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());

        [Test]
        public void Copy_RootElement_From_Request_To_IWithRootElement_Deserializer()
        {
            _server.SetHandler(Handlers.Generic<ResponseHandler>());

            var request = new RestRequest("success")
            {
                RootElement = "Success"
            };
            var deserializer = new CustomDeserializer();
            _client.AddHandler(ContentType.Xml, () => deserializer);
            _client.Execute<Response>(request);

            Assert.AreEqual(deserializer.RootElement, request.RootElement);
        }
    }
}