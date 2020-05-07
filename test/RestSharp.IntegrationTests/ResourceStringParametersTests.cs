using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    public class ResourcestringParametersTests
    {
        SimpleServer _server;

        [SetUp]
        public void SetupServer() => _server = SimpleServer.Create(RequestHandler.Handle);

        [TearDown]
        public void DisposeServer() => _server.Dispose();

        [Test]
        public void Should_keep_to_parameters_with_the_same_name()
        {
            var client     = new RestClient(_server.Url);
            var parameters = "?priority=Low&priority=Medium";
            var request    = new RestRequest(parameters);

            client.Get(request);

            var query = RequestHandler.Url.Query;
            query.Should().Be(parameters);
        }

        static class RequestHandler
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