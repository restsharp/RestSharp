using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using RestSharp.Authenticators.OAuth;
using RestSharp.IntegrationTests.Models;
using Xunit;
using System.Net;
using RestSharp.Contrib;
using RestSharp.Authenticators;
using System.Diagnostics;
using System;

namespace RestSharp.IntegrationTests
{
	public class oAuth1Tests
	{
		[Fact(Skip = "Provide your own consumer key/secret before running")]
		public void Can_Authenticate_With_OAuth()
		{
			const string consumerKey = "";
			const string consumerSecret = "";

			var baseUrl = "http://api.twitter.com";
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
			request = new RestRequest("oauth/access_token", Method.POST);
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

			//request = new RestRequest("statuses/update.json", Method.POST);
			//request.AddParameter("status", "Hello world! " + DateTime.Now.Ticks.ToString());
			//client.Authenticator = OAuth1Authenticator.ForProtectedResource(
			//    consumerKey, consumerSecret, oauth_token, oauth_token_secret
			//);

			//response = client.Execute(request);

			//Assert.NotNull(response);
			//Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

        [Fact]
        public void Properly_Encodes_Parameter_Names()
        {
            var postData = new WebParameterCollection { { "name[first]", "Chuck" }, { "name[last]", "Testa" }};
            var sortedParams = OAuthTools.SortParametersExcludingSignature(postData);

            Assert.Equal("name%5Bfirst%5D", sortedParams[0].Name);
        }

		[Fact]
		public void Use_RFC_3986_Encoding_For_Auth_Signature_Base()
		{
			// reserved characters for 2396 and 3986
			var reserved2396Characters = new[] { ";", "/", "?", ":", "@", "&", "=", "+", "$", "," }; // http://www.ietf.org/rfc/rfc2396.txt
			var additionalReserved3986Characters = new[] { "!", "*", "'", "(", ")" };  // http://www.ietf.org/rfc/rfc3986.txt
			var reservedCharacterString = string.Join( string.Empty, reserved2396Characters.Union( additionalReserved3986Characters ) );
			
			// act
			var escapedString = OAuthTools.UrlEncodeRelaxed( reservedCharacterString );

			// assert
			Assert.Equal( "%3B%2F%3F%3A%40%26%3D%2B%24%2C%21%2A%27%28%29", escapedString );
		}

		[Fact( Skip = "Provide your own consumer key/secret before running" )]
		public void Can_Authenticate_LinkedIN_With_OAuth()
		{
			const string consumerKey = "TODO_CONSUMER_KEY_HERE";
			const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

			// request token
			var client = new RestClient {
				BaseUrl = "https://api.linkedin.com/uas/oauth",
				Authenticator = OAuth1Authenticator.ForRequestToken( consumerKey, consumerSecret, "http://localhost" )
			};
			var requestTokenRequest = new RestRequest( "requestToken" );
			var requestTokenResponse = client.Execute( requestTokenRequest );
			Assert.NotNull( requestTokenResponse );
			Assert.Equal( HttpStatusCode.OK, requestTokenResponse.StatusCode );
			var requestTokenResponseParameters = HttpUtility.ParseQueryString( requestTokenResponse.Content );
			var requestToken = requestTokenResponseParameters[ "oauth_token" ];
			var requestSecret = requestTokenResponseParameters[ "oauth_token_secret" ];
			Assert.NotNull( requestToken );
			Assert.NotNull( requestSecret );

			// redirect user
			requestTokenRequest = new RestRequest( "authenticate?oauth_token=" + requestToken );
			var redirectUri = client.BuildUri( requestTokenRequest );
			Process.Start( redirectUri.ToString() );
			var requestUrl = "TODO: put browser URL here"; // replace this via the debugger with the return url from LinkedIN. Simply copy it from the opened browser
			if ( !Debugger.IsAttached )
			{
				Debugger.Launch();
			}
			Debugger.Break();

			// get the access token
			var requestTokenQueryParameters = HttpUtility.ParseQueryString( new Uri( requestUrl ).Query );
			var requestVerifier = requestTokenQueryParameters[ "oauth_verifier" ];
			client.Authenticator = OAuth1Authenticator.ForAccessToken( consumerKey, consumerSecret, requestToken, requestSecret, requestVerifier );
			var requestAccessTokenRequest = new RestRequest( "accessToken" );
			var requestActionTokenResponse = client.Execute( requestAccessTokenRequest );
			Assert.NotNull( requestActionTokenResponse );
			Assert.Equal( HttpStatusCode.OK, requestActionTokenResponse.StatusCode );
			var requestActionTokenResponseParameters = HttpUtility.ParseQueryString( requestActionTokenResponse.Content );
			var accessToken = requestActionTokenResponseParameters[ "oauth_token" ];
			var accessSecret = requestActionTokenResponseParameters[ "oauth_token_secret" ];
			Assert.NotNull( accessToken );
			Assert.NotNull( accessSecret );
		}

		[Fact( Skip = "Provide your own consumer key/secret/accessToken/accessSecret before running. You can retrieve the access token/secret by running the LinkedIN oAuth test" )]
		public void Can_Retrieve_Member_Profile_Field_Field_Selector_From_LinkedIN()
		{
			const string consumerKey = "TODO_CONSUMER_KEY_HERE";
			const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";
			const string accessToken = "TODO_ACCES_TOKEN_HERE";
			const string accessSecret = "TODO_ACCES_SECRET_HERE";

			// arrange
			var client = new RestClient {
				BaseUrl = "http://api.linkedin.com/v1",
				Authenticator = OAuth1Authenticator.ForProtectedResource( consumerKey, consumerSecret, accessToken, accessSecret )
			};
			var request = new RestRequest( "people/~:(id,first-name,last-name)" );

			// act
			var response = client.Execute< LinkedINMemberProfile >( request );

			// assert
			Assert.NotNull( response );
			Assert.Equal( HttpStatusCode.OK, response.StatusCode );
			Assert.NotNull( response.Data );
			Assert.NotNull( response.Data.Id );
			Assert.NotNull( response.Data.FirstName );
			Assert.NotNull( response.Data.LastName );
		}

		[Fact( Skip = "Provide your own consumer key/secret before running" )]
		public void Can_Query_Vimeo()
		{
			const string consumerKey = "TODO_CONSUMER_KEY_HERE";
			const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

			// arrange
			var client = new RestClient {
				BaseUrl = "http://vimeo.com/api/rest/v2",
				Authenticator = OAuth1Authenticator.ForRequestToken( consumerKey, consumerSecret )
			};
			var request = new RestRequest();
			request.AddParameter( "format", "json" );
			request.AddParameter( "method", "vimeo.videos.search" );
			request.AddParameter( "query", "weather" );
			request.AddParameter( "full_response", 1 );

			// act
			var response = client.Execute( request );

			// assert
			Assert.NotNull( response );
			Assert.Equal( HttpStatusCode.OK, response.StatusCode );
			Assert.NotNull( response.Content );
			Assert.False( response.Content.Contains( "\"stat\":\"fail\"" )  );
			Assert.True( response.Content.Contains( "\"stat\":\"ok\"" )  );
		}
	}
}
