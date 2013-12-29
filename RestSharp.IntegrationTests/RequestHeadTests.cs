using System.Collections.Specialized;
using System.Linq;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    public class RequestHeadTests
    {
        private const string BASE_URL = "http://localhost:8080/";

        public RequestHeadTests()
        {
            RequestHeadCapturer.Initialize();
        }

        [Fact]
        public void Does_Not_Pass_Default_Credentials_When_Server_Does_Not_Negotiate()
        {
            const Method httpMethod = Method.GET;
            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestHeadCapturer>()))
            {
                var client = new RestClient(BASE_URL);
                var request = new RestRequest(RequestHeadCapturer.RESOURCE, httpMethod)
                {
                    UseDefaultCredentials = true
                };

                client.Execute(request);

                Assert.NotNull(RequestHeadCapturer.CapturedHeaders);
                var keys = RequestHeadCapturer.CapturedHeaders.Keys.Cast<string>().ToArray();
                Assert.False(keys.Contains("Authorization"), "Authorization header was present in HTTP request from client, even though server does not use the Negotiate scheme");
            }
        }

        [Fact]
        public void Passes_Default_Credentials_When_UseDefaultCredentials_Is_True()
        {
            const Method httpMethod = Method.GET;
            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate))
            {
                var client = new RestClient(BASE_URL);
                var request = new RestRequest(RequestHeadCapturer.RESOURCE, httpMethod)
                {
                    UseDefaultCredentials = true
                };

                var response = client.Execute(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(RequestHeadCapturer.CapturedHeaders);
                var keys = RequestHeadCapturer.CapturedHeaders.Keys.Cast<string>().ToArray();
                Assert.True(keys.Contains("Authorization"), "Authorization header not present in HTTP request from client, even though UseDefaultCredentials = true");
            }
        }

        [Fact]
        public void Does_Not_Pass_Default_Credentials_When_UseDefaultCredentials_Is_False()
        {
            const Method httpMethod = Method.GET;
            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate))
            {
                var client = new RestClient(BASE_URL);
                var request = new RestRequest(RequestHeadCapturer.RESOURCE, httpMethod)
                {
                    // UseDefaultCredentials is currently false by default, but to make the test more robust in case that ever
                    // changes, it's better to explicitly set it here.
                    UseDefaultCredentials = false
                };

                var response = client.Execute(request);

                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Null(RequestHeadCapturer.CapturedHeaders);
            }
        }

        private class RequestHeadCapturer
        {
            public const string RESOURCE = "Capture";

            public static NameValueCollection CapturedHeaders { get; set; }

            public static void Initialize()
            {
                CapturedHeaders = null;
            }

            public static void Capture(HttpListenerContext context)
            {
                var request = context.Request;
                CapturedHeaders = request.Headers;
            }
        }
    }
}