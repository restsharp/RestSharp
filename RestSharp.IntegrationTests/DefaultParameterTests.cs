using System;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using Shouldly;

namespace RestSharp.IntegrationTests
{
    public class DefaultParameterTests
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
        public void Should_add_default_and_request_query_get_parameters()
        {
            var client = new RestClient(BASE_URL).AddDefaultParameter("foo", "bar", ParameterType.QueryString);
            var request = new RestRequest().AddParameter("foo1", "bar1", ParameterType.QueryString);

            client.Get(request);

            var query = RequestHandler.Url.Query;
            query.ShouldContain("foo=bar");
            query.ShouldContain("foo1=bar1");
        }

        [Test]
        public void Should_add_default_and_request_url_get_parameters()
        {
            var client = new RestClient(BASE_URL + "{foo}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
            var request = new RestRequest("{foo1}").AddParameter("foo1", "bar1", ParameterType.UrlSegment);

            client.Get(request);

            RequestHandler.Url.Segments.ShouldContain("/bar");
            RequestHandler.Url.Segments.ShouldContain("bar1");
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