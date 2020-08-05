using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using RestSharp.Authenticators;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;
using RestSharp.Validation;

namespace RestSharp.InteractiveTests
{
    public class AuthenticationTests
    {
        public class TwitterKeys
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
        }

        public static void Can_Authenticate_With_OAuth(TwitterKeys twitterKeys)
        {
            Console.WriteLine("OAuth test");
            
            var baseUrl = new Uri("https://api.twitter.com");

            Console.WriteLine("Getting request token...");
            var client = new RestClient(baseUrl)
            {
                Authenticator = OAuth1Authenticator.ForRequestToken(twitterKeys.ConsumerKey, twitterKeys.ConsumerSecret)
            };
            var request  = new RestRequest("oauth/request_token");
            var response = client.Execute(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var qs               = HttpUtility.ParseQueryString(response.Content);
            var oauthToken       = qs["oauth_token"];
            var oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("oauth/authorize?oauth_token=" + oauthToken);

            var url = client.BuildUri(request)
                .ToString();

            Console.WriteLine($"Open this URL in the browser: {url} and complete the authentication.");
            Console.Write("Enter the verifier: ");
            var verifier = Console.ReadLine();

            Console.WriteLine("Getting access token...");
            request = new RestRequest("oauth/access_token");

            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                twitterKeys.ConsumerKey,
                twitterKeys.ConsumerSecret,
                oauthToken,
                oauthTokenSecret,
                verifier!
            );
            response = client.Execute(request);

            Console.WriteLine($"Code: {response.StatusCode}, response: {response.Content}");
            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            qs               = HttpUtility.ParseQueryString(response.Content);
            oauthToken       = qs["oauth_token"];
            oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            Console.WriteLine("Verifying credentials...");
            request = new RestRequest("1.1/account/verify_credentials.json", DataFormat.Json);

            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                twitterKeys.ConsumerKey,
                twitterKeys.ConsumerSecret,
                oauthToken,
                oauthTokenSecret
            );
            response = client.Execute(request);

            Console.WriteLine($"Code: {response.StatusCode}, response: {response.Content}");
        }

        public static async Task Can_Authenticate_With_OAuth_Async_With_Callback(TwitterKeys twitterKeys)
        {
            Console.WriteLine("OAuth test with callback");
            
            var baseUrl = new Uri("https://api.twitter.com");

            var client = new RestClient(baseUrl)
            {
                Authenticator = OAuth1Authenticator.ForRequestToken(
                    twitterKeys.ConsumerKey,
                    twitterKeys.ConsumerSecret,
                    "https://restsharp.dev"
                )
            };
            var request  = new RestRequest("oauth/request_token");
            var response = await client.ExecuteAsync(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var qs               = HttpUtility.ParseQueryString(response.Content);
            var oauthToken       = qs["oauth_token"];
            var oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("oauth/authorize?oauth_token=" + oauthToken);

            var url = client.BuildUri(request)
                .ToString();

            Console.WriteLine($"Open this URL in the browser: {url} and complete the authentication.");
            Console.Write("Enter the verifier: ");
            var verifier = Console.ReadLine();

            request = new RestRequest("oauth/access_token");

            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                twitterKeys.ConsumerKey,
                twitterKeys.ConsumerSecret,
                oauthToken,
                oauthTokenSecret,
                verifier!
            );
            response = await client.ExecuteAsync(request);

            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            qs               = HttpUtility.ParseQueryString(response.Content);
            oauthToken       = qs["oauth_token"];
            oauthTokenSecret = qs["oauth_token_secret"];

            Assert.NotNull(oauthToken);
            Assert.NotNull(oauthTokenSecret);

            request = new RestRequest("1.1/account/verify_credentials.json");

            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                twitterKeys.ConsumerKey,
                twitterKeys.ConsumerSecret,
                oauthToken,
                oauthTokenSecret
            );
            response = await client.ExecuteAsync(request);

            Console.WriteLine($"Code: {response.StatusCode}, response: {response.Content}");
        }
    }
}
