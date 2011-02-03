using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Contrib;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class AuthenticationTests
	{
		[Fact]
		public void Can_Authenticate_With_Basic_Http_Auth()
		{
			const string baseUrl = "http://localhost:8080/";
			using(SimpleServer.Create(baseUrl, UsernamePasswordEchoHandler))
			{
				var client = new RestClient(baseUrl);
				client.Authenticator = new HttpBasicAuthenticator("testuser", "testpassword");

				var request = new RestRequest("test");
				var response = client.Execute(request);

				Assert.Equal("testuser|testpassword", response.Content);
			}
		}

		private static void UsernamePasswordEchoHandler(HttpListenerContext context)
		{
			var header = context.Request.Headers["Authorization"];

			var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length))).Split(':');
			context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
		}

		//[Fact]
		public void Can_Authenticate_With_OAuth()
		{
			var baseUrl = "https://api.twitter.com";
			var client = new RestClient(baseUrl);
			client.Authenticator = OAuth1Authenticator.ForRequestToken(
				"CONSUMER_KEY", "CONSUMER_SECRET"
				);
			var request = new RestRequest("oauth/request_token");
			var response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var qs = HttpUtility.ParseQueryString(response.Content);
			var oauth_token = qs["oauth_token"];
			var oauth_token_secret = qs["oauth_token_secret"];
			Assert.NotNull(oauth_token);
			Assert.NotNull(oauth_token_secret);

			request = new RestRequest("oauth/authorize?oauth_token=" + oauth_token);
			var url = client.BuildUri(request).ToString();
			Process.Start(url);

			var verifier = "123456"; // <-- Breakpoint here (set verifier in debugger)
			request = new RestRequest("oauth/access_token");
			client.Authenticator = OAuth1Authenticator.ForAccessToken(
				"P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret, verifier
				);
			response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			qs = HttpUtility.ParseQueryString(response.Content);
			oauth_token = qs["oauth_token"];
			oauth_token_secret = qs["oauth_token_secret"];
			Assert.NotNull(oauth_token);
			Assert.NotNull(oauth_token_secret);

			request = new RestRequest("account/verify_credentials.xml");
			client.Authenticator = OAuth1Authenticator.ForProtectedResource(
				"P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret
				);

			response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		//[Fact]
		//public void Can_Obtain_OAuth_Request_Token()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret");
		//    var request = new RestRequest("request_token.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("oauth_token=requestkey&oauth_token_secret=requestsecret", response.Content);
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
		//    Assert.Equal("oauth_token=accesskey&oauth_token_secret=accesssecret", response.Content);

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
		//    Assert.Equal("fizz=pop&foo=bar", response.Content);
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
		//    Assert.Equal(string.Empty, response.Content);

		//}

	}
}
