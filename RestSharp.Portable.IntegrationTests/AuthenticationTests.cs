using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
//using RestSharp.Authenticators;
//using RestSharp.Contrib;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    [Trait("Integration", "Authentication Tests")]
	public class AuthenticationTests
	{
        const string baseUrl = "http://localhost:8080/";

        [Fact]
        
        public void Does_Not_Pass_Default_Credentials_When_Server_Does_Not_Negotiate()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestCapturer>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/Capture");
                request.UseDefaultCredentials = true;

                var result = client.ExecuteAsync(request);
                result.Wait();
                var response = result.Result;

                Assert.NotNull(RequestCapturer.Headers);
                Assert.NotNull(RequestCapturer.Headers);
                Assert.Null(RequestCapturer.Headers["Authorization"]);
            }
        }

        
        public void Pass_Default_Credentials_When_UseDefaultCredentials_Is_True()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestCapturer>(), AuthenticationSchemes.Negotiate))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/Capture");
                request.UseDefaultCredentials = true;

                var result = client.ExecuteAsync(request);
                result.Wait();
                var response = result.Result;

                System.Diagnostics.Debug.WriteLine(response.StatusDescription);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(RequestCapturer.Headers);
                Assert.NotNull(RequestCapturer.Headers["Authorization"]);
            }
        }

        
        public void Does_Not_Pass_Default_Credentials_When_UseDefaultCredentials_Is_False()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestCapturer>(), AuthenticationSchemes.Negotiate))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/Capture");

                // UseDefaultCredentials is currently false by default, but to make the test more robust in case that ever
                // changes, it's better to explicitly set it here.
                request.UseDefaultCredentials = false;

                var result = client.ExecuteAsync(request);
                result.Wait();
                var response = result.Result;

                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Null(RequestCapturer.Headers);
            }
        }

        
        public async void Can_Execute_GET_With_Basic_Http_Authentication()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoBasicAuthCredentialsValue()))
            {
                var client = new RestClient(baseUrl);
                client.Authenticator = new HttpBasicAuthenticator("testuser", "testpassword");

                var request = new RestRequest("test");
                var response = await client.ExecuteAsync(request);

                Assert.NotNull(response.Content);
                Assert.Equal("testuser|testpassword", response.Content);
            }
        }


		//[Fact]
        //public async void Can_Authenticate_With_OAuth()
        //{
        //    var baseUrl = "https://api.twitter.com";
        //    var client = new RestClient(baseUrl);
        //    client.Authenticator = OAuth1Authenticator.ForRequestToken(
        //        "CONSUMER_KEY", "CONSUMER_SECRET"
        //        );
        //    var request = new RestRequest("oauth/request_token");
        //    var response = await client.ExecuteAsync(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        //    var qs = HttpUtility.ParseQueryString(response.Content);
        //    var oauth_token = qs["oauth_token"];
        //    var oauth_token_secret = qs["oauth_token_secret"];
        //    Assert.NotNull(oauth_token);
        //    Assert.NotNull(oauth_token_secret);

        //    request = new RestRequest("oauth/authorize?oauth_token=" + oauth_token);
        //    var url = client.BuildUri(request).ToString();
        //    Process.Start(url);

        //    var verifier = "123456"; // <-- Breakpoint here (set verifier in debugger)
        //    request = new RestRequest("oauth/access_token");
        //    client.Authenticator = OAuth1Authenticator.ForAccessToken(
        //        "P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret, verifier
        //        );
        //    response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        //    qs = HttpUtility.ParseQueryString(response.Content);
        //    oauth_token = qs["oauth_token"];
        //    oauth_token_secret = qs["oauth_token_secret"];
        //    Assert.NotNull(oauth_token);
        //    Assert.NotNull(oauth_token_secret);

        //    request = new RestRequest("account/verify_credentials.xml");
        //    client.Authenticator = OAuth1Authenticator.ForProtectedResource(
        //        "P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret
        //        );

        //    response = client.Execute(request);

        //    Assert.NotNull(response);
        //    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        //}

		//[Fact]
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

		//[Fact]
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

		//[Fact]
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

		//[Fact]
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
