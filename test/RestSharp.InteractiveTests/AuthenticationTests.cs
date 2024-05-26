using System.Net;
using System.Web;
using RestSharp.Authenticators;

namespace RestSharp.InteractiveTests;

public class AuthenticationTests {
    public class TwitterKeys {
        public string ConsumerKey    { get; set; }
        public string ConsumerSecret { get; set; }
    }

    public static async Task Can_Authenticate_With_OAuth_Async_With_Callback(TwitterKeys twitterKeys) {
        Console.WriteLine("OAuth test with callback");

        var baseUrl = new Uri("https://api.twitter.com");

        using var client = new RestClient(
            baseUrl,
            options =>
                options.Authenticator = OAuth1Authenticator.ForRequestToken(
                    twitterKeys.ConsumerKey!,
                    twitterKeys.ConsumerSecret,
                    "https://restsharp.dev"
                )
        );
        var request  = new RestRequest("oauth/request_token");
        var response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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

        request = new RestRequest("oauth/access_token") {
            Authenticator = OAuth1Authenticator.ForAccessToken(
                twitterKeys.ConsumerKey!,
                twitterKeys.ConsumerSecret,
                oauthToken!,
                oauthTokenSecret!,
                verifier!
            )
        };

        response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        qs               = HttpUtility.ParseQueryString(response.Content);
        oauthToken       = qs["oauth_token"];
        oauthTokenSecret = qs["oauth_token_secret"];

        Assert.NotNull(oauthToken);
        Assert.NotNull(oauthTokenSecret);

        request = new RestRequest("1.1/account/verify_credentials.json") {
            Authenticator = OAuth1Authenticator.ForProtectedResource(
                twitterKeys.ConsumerKey!,
                twitterKeys.ConsumerSecret,
                oauthToken!,
                oauthTokenSecret!
            )
        };

        response = await client.ExecuteAsync(request);

        Console.WriteLine($"Code: {response.StatusCode}, response: {response.Content}");
    }
}
