using System.Collections.Generic;
using System.Xml.Serialization;
using Xunit;
using System.Net;
using RestSharp.Contrib;
using RestSharp.Authenticators;
using System.Diagnostics;

namespace RestSharp.IntegrationTests
{
	public class oAuth1Tests
	{
		//[Fact]
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

		#region Netflix test classes

		[XmlRoot("queue")]
		private class Queue
		{
			[XmlElement("etag")]
			public string Etag { get; set; }

			public List<QueueItem> Items { get; set; }
		}

		[XmlRoot("queue_item")]
		private class QueueItem
		{
			[XmlElement("id")]
			public string ID { get; set; }

			[XmlElement("position")]
			public int Position { get; set; }
		}

		#endregion

		//[Fact]
		public void Can_Authenticate_Netflix_With_OAuth()
		{
			const string consumerKey = "";
			const string consumerSecret = "";

			var baseUrl = "http://api.netflix.com";
			var client = new RestClient(baseUrl);
			client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
			var request = new RestRequest("oauth/request_token");
			var response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var qs = HttpUtility.ParseQueryString(response.Content);
			var oauth_token = qs["oauth_token"];
			var oauth_token_secret = qs["oauth_token_secret"];
			var applicationName = qs["application_name"];
			Assert.NotNull(oauth_token);
			Assert.NotNull(oauth_token_secret);
			Assert.NotNull(applicationName);

			var baseSslUrl = "https://api-user.netflix.com";
			var sslClient = new RestClient(baseSslUrl);
			request = new RestRequest("oauth/login");
			request.AddParameter("oauth_token", oauth_token);
			request.AddParameter("oauth_consumer_key", consumerKey);
			request.AddParameter("application_name", applicationName);
			var url = sslClient.BuildUri(request).ToString();
			
			Process.Start(url);

			request = new RestRequest("oauth/access_token"); // <-- Breakpoint here, login to netflix
			client.Authenticator = OAuth1Authenticator.ForAccessToken(
				consumerKey, consumerSecret, oauth_token, oauth_token_secret
			);
			response = client.Execute(request);

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			qs = HttpUtility.ParseQueryString(response.Content);
			oauth_token = qs["oauth_token"];
			oauth_token_secret = qs["oauth_token_secret"];
			var user_id = qs["user_id"];
			Assert.NotNull(oauth_token);
			Assert.NotNull(oauth_token_secret);
			Assert.NotNull(user_id);

			client.Authenticator = OAuth1Authenticator.ForProtectedResource(consumerKey, consumerSecret, oauth_token, oauth_token_secret);
			request = new RestRequest("users/{user_id}/queues/disc");
			request.AddUrlSegment("user_id", user_id);
			request.AddParameter("max_results", "2");
			var queueResponse = client.Execute<Queue>(request);

			Assert.NotNull(queueResponse);
			Assert.Equal(HttpStatusCode.OK, queueResponse.StatusCode);

			Assert.NotNull(queueResponse.Data);
			Assert.Equal(2, queueResponse.Data.Items.Count);
		}
	}
}
