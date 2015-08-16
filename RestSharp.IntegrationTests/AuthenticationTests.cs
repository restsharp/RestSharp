using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using NUnit.Framework;
using RestSharp.Authenticators;
using RestSharp.Extensions.MonoHttp;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class AuthenticationTests
    {
        [Test]
        public void Can_Authenticate_With_Basic_Http_Auth()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, UsernamePasswordEchoHandler))
            {
                RestClient client = new RestClient(baseUrl)
                                    {
                                        Authenticator = new HttpBasicAuthenticator("testuser", "testpassword")
                                    };
                RestRequest request = new RestRequest("test");
                IRestResponse response = client.Execute(request);

                Assert.AreEqual("testuser|testpassword", response.Content);
            }
        }

        private static void UsernamePasswordEchoHandler(HttpListenerContext context)
        {
            string header = context.Request.Headers["Authorization"];
            string[] parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length)))
                                     .Split(':');

            context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
        }

        //[Test]
        public void Can_Authenticate_With_OAuth()
        {
            Uri baseUrl = new Uri("https://api.twitter.com");
            RestClient client = new RestClient(baseUrl)
                                {
                                    Authenticator = OAuth1Authenticator.ForRequestToken("CONSUMER_KEY", "CONSUMER_SECRET")
                                };
            RestRequest request = new RestRequest("oauth/request_token");
            IRestResponse response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            NameValueCollection qs = HttpUtility.ParseQueryString(response.Content);
            string oauthToken = qs["oauth_token"];
            string oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("oauth/authorize?oauth_token=" + oauthToken);

            string url = client.BuildUri(request)
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

        //[Test]
        //public void Can_Obtain_OAuth_Request_Token()
        //{
        //    var baseUrl = "http://term.ie/oauth/example";
        //    var client = new RestClient(baseUrl);

        //    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret");

        //    var request = new RestRequest("request_token.php");
        //    var response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual("oauth_token=requestkey&oauth_token_secret=requestsecret", response.Content);
        //}

        //[Test]
        //public void Can_Obtain_OAuth_Access_Token()
        //{
        //    var baseUrl = "http://term.ie/oauth/example";
        //    var client = new RestClient(baseUrl);

        //    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "requestkey", "requestsecret");

        //    var request = new RestRequest("access_token.php");
        //    var response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual("oauth_token=accesskey&oauth_token_secret=accesssecret", response.Content);

        //}

        //[Test]
        //public void Can_Make_Authenticated_OAuth_Call_With_Parameters()
        //{
        //    var baseUrl = "http://term.ie/oauth/example";
        //    var client = new RestClient(baseUrl);

        //    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");

        //    var request = new RestRequest("echo_api.php");

        //    request.AddParameter("foo", "bar");
        //    request.AddParameter("fizz", "pop");

        //    var response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual("fizz=pop&foo=bar", response.Content);
        //}

        //[Test]
        //public void Can_Make_Authenticated_OAuth_Call()
        //{
        //    var baseUrl = "http://term.ie/oauth/example";
        //    var client = new RestClient(baseUrl);

        //    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");

        //    var request = new RestRequest("echo_api.php");
        //    var response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual(string.Empty, response.Content);

        //}
    }
}
