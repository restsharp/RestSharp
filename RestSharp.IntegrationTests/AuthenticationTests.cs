using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using NUnit.Framework;
using RestSharp.Authenticators;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AuthenticationTests
    {
        private static void UsernamePasswordEchoHandler(HttpListenerContext context)
        {
            var header = context.Request.Headers["Authorization"];
            var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length)))
                .Split(':');

            context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
        }

        //[Test]
        public void Can_Authenticate_With_OAuth()
        {
            var baseUrl = new Uri("https://api.twitter.com");
            var client = new RestClient(baseUrl)
            {
                Authenticator = OAuth1Authenticator.ForRequestToken("CONSUMER_KEY", "CONSUMER_SECRET")
            };
            var request = new RestRequest("oauth/request_token");
            var response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var qs = HttpUtility.ParseQueryString(response.Content);
            var oauthToken = qs["oauth_token"];
            var oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("oauth/authorize?oauth_token=" + oauthToken);

            var url = client.BuildUri(request)
                .ToString();

            Process.Start(url);

            const string verifier = "123456"; // <-- Breakpoint here (set verifier in debugger)

            request = new RestRequest("oauth/access_token");
            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                "P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauthToken,
                oauthTokenSecret, verifier);
            response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            qs = HttpUtility.ParseQueryString(response.Content);
            oauthToken = qs["oauth_token"];
            oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("account/verify_credentials.xml");
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                "P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauthToken,
                oauthTokenSecret);
            response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void Can_Authenticate_With_Basic_Http_Auth()
        {
            var baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, UsernamePasswordEchoHandler))
            {
                var client = new RestClient(baseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator("testuser", "testpassword")
                };
                var request = new RestRequest("test");
                var response = client.Execute(request);

                Assert.AreEqual("testuser|testpassword", response.Content);
            }
        }
    }
}