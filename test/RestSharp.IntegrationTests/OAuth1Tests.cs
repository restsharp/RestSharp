using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using RestSharp.IntegrationTests.Models;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class OAuth1Tests
    {
        [XmlRoot("queue")]
        class Queue
        {
            [XmlElement("etag")]
            public string Etag { get; set; }

            public List<QueueItem> Items { get; set; }
        }

        [XmlRoot("queue_item")]
        class QueueItem
        {
            [XmlElement("id")]
            public string Id { get; set; }

            [XmlElement("position")]
            public int Position { get; set; }
        }

        [Test]
        [Ignore("Needs Netflix token")]
        public void Can_Authenticate_Netflix_With_OAuth()
        {
            const string consumerKey    = "";
            const string consumerSecret = "";

            var baseUrl = new Uri("http://api.netflix.com");

            var client = new RestClient(baseUrl)
            {
                Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
            };
            var request  = new RestRequest("oauth/request_token");
            var response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var qs               = HttpUtility.ParseQueryString(response.Content);
            var oauthToken       = qs["oauth_token"];
            var oauthTokenSecret = qs["oauth_token_secret"];
            var applicationName  = qs["application_name"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);
            Assert.NotNull(applicationName);

            var baseSslUrl = new Uri("https://api-user.netflix.com");
            var sslClient  = new RestClient(baseSslUrl);

            request = new RestRequest("oauth/login");
            request.AddParameter("oauth_token", oauthToken);
            request.AddParameter("oauth_consumer_key", consumerKey);
            request.AddParameter("application_name", applicationName);

            var url = sslClient.BuildUri(request)
                .ToString();

            Process.Start(url);

            request = new RestRequest("oauth/access_token"); // <-- Breakpoint here, login to netflix

            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey, consumerSecret, oauthToken,
                oauthTokenSecret
            );
            response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            qs               = HttpUtility.ParseQueryString(response.Content);
            oauthToken       = qs["oauth_token"];
            oauthTokenSecret = qs["oauth_token_secret"];

            var userId = qs["user_id"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);
            Assert.NotNull(userId);

            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                consumerKey, consumerSecret, oauthToken,
                oauthTokenSecret
            );
            request = new RestRequest("users/{user_id}/queues/disc");
            request.AddUrlSegment("user_id", userId);
            request.AddParameter("max_results", "2");

            var queueResponse = client.Execute<Queue>(request);

            Assert.NotNull(queueResponse);
            Assert.AreEqual(HttpStatusCode.OK, queueResponse.StatusCode);
            Assert.NotNull(queueResponse.Data);
            Assert.AreEqual(2, queueResponse.Data.Items.Count);
        }

        [Test]
        [Ignore("Provide your own consumer key/secret before running")]
        public void Can_Authenticate_LinkedIN_With_OAuth()
        {
            const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
            const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

            // request token
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.linkedin.com/uas/oauth"),
                Authenticator = OAuth1Authenticator.ForRequestToken(
                    consumerKey, consumerSecret,
                    "http://localhost"
                )
            };
            var requestTokenRequest  = new RestRequest("requestToken");
            var requestTokenResponse = client.Execute(requestTokenRequest);

            Assert.NotNull(requestTokenResponse);
            Assert.AreEqual(HttpStatusCode.OK, requestTokenResponse.StatusCode);

            var requestTokenResponseParameters = HttpUtility.ParseQueryString(requestTokenResponse.Content);
            var requestToken                   = requestTokenResponseParameters["oauth_token"];
            var requestSecret                  = requestTokenResponseParameters["oauth_token_secret"];

            Assert.NotNull(requestToken);
            Assert.NotNull(requestSecret);

            // redirect user
            requestTokenRequest = new RestRequest("authenticate?oauth_token=" + requestToken);

            var redirectUri = client.BuildUri(requestTokenRequest);

            Process.Start(redirectUri.ToString());

            const string requestUrl = "TODO: put browser URL here";
            // replace this via the debugger with the return url from LinkedIN. Simply copy it from the opened browser

            if (!Debugger.IsAttached)
                Debugger.Launch();

            Debugger.Break();

            // get the access token
            var requestTokenQueryParameters = HttpUtility.ParseQueryString(new Uri(requestUrl).Query);
            var requestVerifier             = requestTokenQueryParameters["oauth_verifier"];

            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey, consumerSecret, requestToken, requestSecret, requestVerifier
            );

            var requestAccessTokenRequest  = new RestRequest("accessToken");
            var requestActionTokenResponse = client.Execute(requestAccessTokenRequest);

            Assert.NotNull(requestActionTokenResponse);
            Assert.AreEqual(HttpStatusCode.OK, requestActionTokenResponse.StatusCode);

            var requestActionTokenResponseParameters = HttpUtility.ParseQueryString(requestActionTokenResponse.Content);
            var accessToken                          = requestActionTokenResponseParameters["oauth_token"];
            var accessSecret                         = requestActionTokenResponseParameters["oauth_token_secret"];

            Assert.NotNull(accessToken);
            Assert.NotNull(accessSecret);
        }

        [Test]
        public void Can_Authenticate_OAuth1_With_Querystring_Parameters()
        {
            const string consumerKey    = "enterConsumerKeyHere";
            const string consumerSecret = "enterConsumerSecretHere";
            const string baseUrl        = "http://restsharp.org";

            var expected = new List<string>
            {
                "oauth_consumer_key",
                "oauth_nonce",
                "oauth_signature_method",
                "oauth_timestamp",
                "oauth_version",
                "oauth_signature"
            };

            var client        = new RestClient(baseUrl);
            var request       = new RestRequest(Method.GET);
            var authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;
            authenticator.Authenticate(client, request);

            var requestUri = client.BuildUri(request);
            var actual     = HttpUtility.ParseQueryString(requestUri.Query).AllKeys.ToList();

            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [Test]
        [Ignore("Provide your own consumer key/secret before running")]
        public void Can_Authenticate_Twitter()
        {
            var config = new
            {
                ConsumerKey    = "",
                ConsumerSecret = "",
                AccessToken    = "",
                AccessSecret   = ""
            };

            var client = new RestClient("https://api.twitter.com/1.1")
            {
                Authenticator = OAuth1Authenticator.ForProtectedResource(
                    config.ConsumerKey,
                    config.ConsumerSecret,
                    config.AccessToken,
                    config.AccessSecret
                )
            };

            var request = new RestRequest("account/verify_credentials.json");

            request.AddParameter("include_entities", "true", ParameterType.QueryString);

            var response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Ignore("Provide your own consumer key/secret before running")]
        public void Can_Authenticate_With_OAuth()
        {
            const string consumerKey    = "";
            const string consumerSecret = "";

            var baseUrl = new Uri("https://api.twitter.com");

            var client = new RestClient(baseUrl)
            {
                Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
            };
            var request  = new RestRequest("oauth/request_token", Method.POST);
            var response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var qs               = HttpUtility.ParseQueryString(response.Content);
            var oauthToken       = qs["oauth_token"];
            var oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("oauth/authorize");
            request.AddParameter("oauth_token", oauthToken);

            var url = client.BuildUri(request).ToString();

            // Breakpoint here, open the URL from the url var in the browser
            // then set verifier in debugger to the value in the URL where you get redirected
            var verifier = "123456";

            request = new RestRequest("oauth/access_token", Method.POST);

            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey, consumerSecret, oauthToken,
                oauthTokenSecret, verifier
            );
            response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            qs               = HttpUtility.ParseQueryString(response.Content);
            oauthToken       = qs["oauth_token"];
            oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("/1.1/account/verify_credentials.json");

            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                consumerKey, consumerSecret, oauthToken,
                oauthTokenSecret
            );

            response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Ignore("Provide your own consumer key/secret before running")]
        public void Can_Query_Vimeo()
        {
            const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
            const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

            // arrange
            var client = new RestClient
            {
                BaseUrl       = new Uri("http://vimeo.com/api/rest/v2"),
                Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
            };
            var request = new RestRequest();

            request.AddParameter("format", "json");
            request.AddParameter("method", "vimeo.videos.search");
            request.AddParameter("query", "weather");
            request.AddParameter("full_response", 1);

            // act
            var response = client.Execute(request);

            // assert
            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            Assert.False(response.Content.Contains("\"stat\":\"fail\""));
            Assert.True(response.Content.Contains("\"stat\":\"ok\""));
        }

        [Test]
        [Ignore(
            "Provide your own consumer key/secret/accessToken/accessSecret before running. You can retrieve the access token/secret by running the LinkedIN oAuth test"
        )]
        public void Can_Retrieve_Member_Profile_Field_Field_Selector_From_LinkedIN()
        {
            const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
            const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";
            const string accessToken    = "TODO_ACCES_TOKEN_HERE";
            const string accessSecret   = "TODO_ACCES_SECRET_HERE";

            // arrange
            var client = new RestClient
            {
                BaseUrl = new Uri("http://api.linkedin.com/v1"),
                Authenticator = OAuth1Authenticator.ForProtectedResource(
                    consumerKey,
                    consumerSecret, accessToken, accessSecret
                )
            };
            var request = new RestRequest("people/~:(id,first-name,last-name)");

            // act
            var response = client.Execute<LinkedInMemberProfile>(request);

            // assert
            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Id);
            Assert.NotNull(response.Data.FirstName);
            Assert.NotNull(response.Data.LastName);
        }

        [Test]
        public void Properly_Encodes_Parameter_Names()
        {
            var postData = new WebPairCollection
            {
                {"name[first]", "Chuck"},
                {"name[last]", "Testa"}
            };
            var sortedParams = OAuthTools.SortParametersExcludingSignature(postData);

            sortedParams.First().Should().Be("name%5Bfirst%5D=Chuck");
        }

        [Test]
        public void Use_RFC_3986_Encoding_For_Auth_Signature_Base()
        {
            // reserved characters for 2396 and 3986
            // http://www.ietf.org/rfc/rfc2396.txt
            string[] reserved2396Characters = {";", "/", "?", ":", "@", "&", "=", "+", "$", ","};
            // http://www.ietf.org/rfc/rfc3986.txt
            string[] additionalReserved3986Characters = {"!", "*", "'", "(", ")"};

            var reservedCharacterString = string.Join(
                string.Empty,
                reserved2396Characters.Union(additionalReserved3986Characters)
            );

            // act
            var escapedString = OAuthTools.UrlEncodeRelaxed(reservedCharacterString);

            // assert
            Assert.AreEqual("%3B%2F%3F%3A%40%26%3D%2B%24%2C%2521%252A%2527%2528%2529", escapedString);
        }
    }
}