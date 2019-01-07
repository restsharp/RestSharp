using System;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using Shouldly;

namespace RestSharp.IntegrationTests
{
    public class ResourcestringParametersTests
    {
        private SimpleServer _server;
        private const string BASE_URL = "http://localhost:8888/";

        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(BASE_URL, RequestHandler.Handle);
        }

        [TearDown]
        public void DisposeServer()
        {
            _server.Dispose();
        }

        [Test]
        public void Should_keep_to_parameters_with_the_same_name()
        {
            var client = new RestClient(BASE_URL);
            var parameters = "?priority=Low&priority=Medium";
            var request = new RestRequest(parameters);

            client.Get(request);

            var query = RequestHandler.Url.Query;
            query.ShouldBe(parameters);
        }

        private static class RequestHandler
        {
            public static Uri Url { get; private set; }
            
            public static void Handle(HttpListenerContext context)
            {
                Url = context.Request.Url;
                Handlers.Echo(context);
            }
            
        }
    }
}