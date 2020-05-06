using System.Collections.Specialized;
using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class RequestHeadTests : CaptureFixture
    {
        [Test]
        public void Does_Not_Pass_Default_Credentials_When_Server_Does_Not_Negotiate()
        {
            const Method httpMethod = Method.GET;

            using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

            var client = new RestClient(server.Url);

            var request = new RestRequest(RequestHeadCapturer.Resource, httpMethod)
            {
                UseDefaultCredentials = true
            };

            client.Execute(request);

            Assert.NotNull(RequestHeadCapturer.CapturedHeaders);

            var keys = RequestHeadCapturer.CapturedHeaders.Keys.Cast<string>()
                .ToArray();

            Assert.False(
                keys.Contains("Authorization"),
                "Authorization header was present in HTTP request from client, even though server does not use the Negotiate scheme"
            );
        }

        [Test]
        public void Does_Not_Pass_Default_Credentials_When_UseDefaultCredentials_Is_False()
        {
            const Method httpMethod = Method.GET;

            using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

            var client = new RestClient(server.Url);

            var request = new RestRequest(RequestHeadCapturer.Resource, httpMethod)
            {
                // UseDefaultCredentials is currently false by default,
                // but to make the test more robust in case that ever
                // changes, it's better to explicitly set it here.
                UseDefaultCredentials = false
            };
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(RequestHeadCapturer.CapturedHeaders);
        }

        [Test]
 #if NETCORE
         [Ignore("Not supported for .NET Core")]
 #endif
        public void Passes_Default_Credentials_When_UseDefaultCredentials_Is_True()
        {
            const Method httpMethod = Method.GET;

            using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

            var client = new RestClient(server.Url);

            var request = new RestRequest(RequestHeadCapturer.Resource, httpMethod)
            {
                UseDefaultCredentials = true
            };
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(RequestHeadCapturer.CapturedHeaders);

            var keys = RequestHeadCapturer.CapturedHeaders.Keys.Cast<string>().ToArray();

            Assert.True(
                keys.Contains("Authorization"),
                "Authorization header not present in HTTP request from client, even though UseDefaultCredentials = true"
            );
        }
    }
}