using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Net;
using RestSharp.Contrib;
using RestSharp.Authenticators;
using System.Diagnostics;

namespace RestSharp.IntegrationTests
{
	public class oAuth1Tests
	{
		[Fact]
		public void Can_Authenticate_With_OAuth()
		{
			const string consumerKey = "";
			const string consumerSecret = "";

			var baseUrl = "https://api.twitter.com";
			var client = new RestClient(baseUrl);
			client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
			var request = new RestRequest("oauth/request_token", Method.POST);
			var response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var qs = HttpUtility.ParseQueryString(response.Content);
			var oauth_token = qs["oauth_token"];
			var oauth_token_secret = qs["oauth_token_secret"];
			Assert.NotNull(oauth_token);
			Assert.NotNull(oauth_token_secret);

			request = new RestRequest("oauth/authorize");
			request.AddParameter("oauth_token", oauth_token);
			var url = client.BuildUri(request).ToString();
			Process.Start(url);

			var verifier = "123456"; // <-- Breakpoint here (set verifier in debugger)
			request = new RestRequest("oauth/access_token");
			client.Authenticator = OAuth1Authenticator.ForAccessToken(
				consumerKey, consumerSecret, oauth_token, oauth_token_secret, verifier
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
				consumerKey, consumerSecret, oauth_token, oauth_token_secret
			);

			response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

	}
}
