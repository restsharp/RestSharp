using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using RestSharp.Tests.Integrated.Models;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Integrated;

public class OAuth1Tests {
    [XmlRoot("queue")]
    class Queue {
        [XmlElement("etag")]
        public string Etag { get; set; }

        public List<QueueItem> Items { get; set; }
    }

    [XmlRoot("queue_item")]
    class QueueItem {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("position")]
        public int Position { get; set; }
    }

    [Fact(Skip = "Needs Netflix token")]
    public async Task Can_Authenticate_Netflix_With_OAuth() {
        const string consumerKey    = "";
        const string consumerSecret = "";

        var baseUrl = new Uri("http://api.netflix.com");

        var client = new RestClient(baseUrl) {
            Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
        };
        var request  = new RestRequest("oauth/request_token");
        var response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var qs               = new Uri(response.Content).ParseQuery();
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
            consumerKey,
            consumerSecret,
            oauthToken,
            oauthTokenSecret
        );
        response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        qs               = new Uri(response.Content).ParseQuery();
        oauthToken       = qs["oauth_token"];
        oauthTokenSecret = qs["oauth_token_secret"];

        var userId = qs["user_id"];

        Assert.NotNull(oauthToken);
        Assert.NotNull(oauthTokenSecret);
        Assert.NotNull(userId);

        client.Authenticator = OAuth1Authenticator.ForProtectedResource(
            consumerKey,
            consumerSecret,
            oauthToken,
            oauthTokenSecret
        );
        request = new RestRequest("users/{user_id}/queues/disc");
        request.AddUrlSegment("user_id", userId);
        request.AddParameter("max_results", "2");

        var queueResponse = await client.ExecuteAsync<Queue>(request);

        Assert.NotNull(queueResponse);
        Assert.Equal(HttpStatusCode.OK, queueResponse.StatusCode);
        Assert.NotNull(queueResponse.Data);
        Assert.Equal(2, queueResponse.Data.Items.Count);
    }

    [Fact(Skip = "Provide your own consumer key/secret before running")]
    public async Task Can_Authenticate_LinkedIN_With_OAuth() {
        const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
        const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

        // request token
        var client = new RestClient("https://api.linkedin.com/uas/oauth") {
            Authenticator = OAuth1Authenticator.ForRequestToken(
                consumerKey,
                consumerSecret,
                "http://localhost"
            )
        };
        var requestTokenRequest  = new RestRequest("requestToken");
        var requestTokenResponse = await client.ExecuteAsync(requestTokenRequest);

        Assert.NotNull(requestTokenResponse);
        Assert.Equal(HttpStatusCode.OK, requestTokenResponse.StatusCode);

        var requestTokenResponseParameters = new Uri(requestTokenResponse.Content).ParseQuery();
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
        var requestTokenQueryParameters = new Uri(requestUrl).ParseQuery();
        var requestVerifier             = requestTokenQueryParameters["oauth_verifier"];

        client.Authenticator = OAuth1Authenticator.ForAccessToken(
            consumerKey,
            consumerSecret,
            requestToken,
            requestSecret,
            requestVerifier
        );

        var requestAccessTokenRequest  = new RestRequest("accessToken");
        var requestActionTokenResponse = await client.ExecuteAsync(requestAccessTokenRequest);

        Assert.NotNull(requestActionTokenResponse);
        Assert.Equal(HttpStatusCode.OK, requestActionTokenResponse.StatusCode);

        var requestActionTokenResponseParameters = new Uri(requestActionTokenResponse.Content).ParseQuery();
        var accessToken                          = requestActionTokenResponseParameters["oauth_token"];
        var accessSecret                         = requestActionTokenResponseParameters["oauth_token_secret"];

        Assert.NotNull(accessToken);
        Assert.NotNull(accessSecret);
    }

    [Fact]
    public void Can_Authenticate_OAuth1_With_Querystring_Parameters() {
        const string consumerKey    = "enterConsumerKeyHere";
        const string consumerSecret = "enterConsumerSecretHere";
        const string baseUrl        = "http://restsharp.org";

        var expected = new List<string> {
            "oauth_consumer_key",
            "oauth_nonce",
            "oauth_signature_method",
            "oauth_timestamp",
            "oauth_version",
            "oauth_signature"
        };

        var client        = new RestClient(baseUrl);
        var request       = new RestRequest();
        var authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
        authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;
        authenticator.Authenticate(client, request);

        var requestUri = client.BuildUri(request);
        var actual     = requestUri.ParseQuery().Select(x => x.Key).ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact(Skip = "Provide your own consumer key/secret before running")]
    public async Task Can_Authenticate_Twitter() {
        var config = new {
            ConsumerKey    = "",
            ConsumerSecret = "",
            AccessToken    = "",
            AccessSecret   = ""
        };

        var client = new RestClient("https://api.twitter.com/1.1") {
            Authenticator = OAuth1Authenticator.ForProtectedResource(
                config.ConsumerKey,
                config.ConsumerSecret,
                config.AccessToken,
                config.AccessSecret
            )
        };

        var request = new RestRequest("account/verify_credentials.json");

        request.AddParameter("include_entities", "true", ParameterType.QueryString);

        var response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Provide your own consumer key/secret before running")]
    public async Task Can_Authenticate_With_OAuth() {
        const string consumerKey    = "";
        const string consumerSecret = "";

        var baseUrl = new Uri("https://api.twitter.com");

        var client = new RestClient(baseUrl) {
            Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
        };
        var request  = new RestRequest("oauth/request_token", Method.Post);
        var response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var qs               = new Uri(response.Content).ParseQuery();
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

        request = new RestRequest("oauth/access_token", Method.Post);

        client.Authenticator = OAuth1Authenticator.ForAccessToken(
            consumerKey,
            consumerSecret,
            oauthToken,
            oauthTokenSecret,
            verifier
        );
        response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        qs               = new Uri(response.Content).ParseQuery();
        oauthToken       = qs["oauth_token"];
        oauthTokenSecret = qs["oauth_token_secret"];

        Assert.NotNull(oauthToken);
        Assert.NotNull(oauthTokenSecret);

        request = new RestRequest("/1.1/account/verify_credentials.json");

        client.Authenticator = OAuth1Authenticator.ForProtectedResource(
            consumerKey,
            consumerSecret,
            oauthToken,
            oauthTokenSecret
        );

        response = await client.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Provide your own consumer key/secret before running")]
    public async Task Can_Query_Vimeo() {
        const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
        const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";

        // arrange
        var client = new RestClient("http://vimeo.com/api/rest/v2") {
            Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
        };
        var request = new RestRequest();

        request.AddParameter("format", "json");
        request.AddParameter("method", "vimeo.videos.search");
        request.AddParameter("query", "weather");
        request.AddParameter("full_response", 1);

        // act
        var response = await client.ExecuteAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);
        response.Content.Should().NotContain("\"stat\":\"fail\"");
        response.Content.Should().Contain("\"stat\":\"ok\"");
    }

    [Fact(Skip =
        "Provide your own consumer key/secret/accessToken/accessSecret before running. You can retrieve the access token/secret by running the LinkedIN oAuth test"
    )]
    public async Task Can_Retrieve_Member_Profile_Field_Field_Selector_From_LinkedIN() {
        const string consumerKey    = "TODO_CONSUMER_KEY_HERE";
        const string consumerSecret = "TODO_CONSUMER_SECRET_HERE";
        const string accessToken    = "TODO_ACCES_TOKEN_HERE";
        const string accessSecret   = "TODO_ACCES_SECRET_HERE";

        // arrange
        var client = new RestClient("http://api.linkedin.com/v1") {
            Authenticator = OAuth1Authenticator.ForProtectedResource(
                consumerKey,
                consumerSecret,
                accessToken,
                accessSecret
            )
        };
        var request = new RestRequest("people/~:(id,first-name,last-name)");

        // act
        var response = await client.ExecuteAsync<LinkedInMemberProfile>(request);

        // assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.Id);
        Assert.NotNull(response.Data.FirstName);
        Assert.NotNull(response.Data.LastName);
    }

    [Fact]
    public void Properly_Encodes_Parameter_Names() {
        var postData = new WebPairCollection {
            { "name[first]", "Chuck" },
            { "name[last]", "Testa" }
        };
        var sortedParams = OAuthTools.SortParametersExcludingSignature(postData);

        sortedParams.First().Should().Be("name%5Bfirst%5D=Chuck");
    }

    [Fact]
    public void Use_RFC_3986_Encoding_For_Auth_Signature_Base() {
        // reserved characters for 2396 and 3986
        // http://www.ietf.org/rfc/rfc2396.txt
        string[] reserved2396Characters = { ";", "/", "?", ":", "@", "&", "=", "+", "$", "," };
        // http://www.ietf.org/rfc/rfc3986.txt
        string[] additionalReserved3986Characters = { "!", "*", "'", "(", ")" };

        var reservedCharacterString = string.Join(
            string.Empty,
            reserved2396Characters.Union(additionalReserved3986Characters)
        );

        // act
        var escapedString = OAuthTools.UrlEncodeRelaxed(reservedCharacterString);

        // assert
        Assert.Equal("%3B%2F%3F%3A%40%26%3D%2B%24%2C%2521%252A%2527%2528%2529", escapedString);
    }
}