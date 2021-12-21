using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace RestSharp.Tests;

public class OAuth1AuthenticatorTests {
    public OAuth1AuthenticatorTests()
        => _authenticator = new OAuth1Authenticator {
            CallbackUrl        = "CallbackUrl",
            ClientPassword     = "ClientPassword",
            Type               = OAuthType.ClientAuthentication,
            ClientUsername     = "ClientUsername",
            ConsumerKey        = "ConsumerKey",
            ConsumerSecret     = "ConsumerSecret",
            Realm              = "Realm",
            SessionHandle      = "SessionHandle",
            SignatureMethod    = OAuthSignatureMethod.PlainText,
            SignatureTreatment = OAuthSignatureTreatment.Escaped,
            Token              = "Token",
            TokenSecret        = "TokenSecret",
            Verifier           = "Verifier",
            Version            = "Version"
        };

    readonly OAuth1Authenticator _authenticator;

    [Fact]
    public void Authenticate_ShouldAddAuthorizationAsTextValueToRequest_OnHttpAuthorizationHeaderHandling() {
        // Arrange
        const string url = "https://no-query.string";

        var client  = new RestClient(url);
        var request = new RestRequest();

        _authenticator.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;

        // Act
        _authenticator.Authenticate(client, request);

        // Assert
        var authParameter = request.Parameters.Single(x => x.Name == KnownHeaders.Authorization);
        var value         = (string)authParameter.Value;

        Assert.Contains("OAuth", value);
        Assert.Contains("realm=\"Realm\"", value);
        Assert.Contains("oauth_timestamp=", value);
        Assert.Contains("oauth_signature=\"ConsumerSecret", value);
        Assert.Contains("oauth_nonce=", value);
        Assert.Contains("oauth_consumer_key=\"ConsumerKey\"", value);
        Assert.Contains("oauth_signature_method=\"PLAINTEXT\"", value);
        Assert.Contains("oauth_version=\"Version\"", value);
        Assert.Contains("x_auth_mode=\"client_auth\"", value);
        Assert.Contains("x_auth_username=\"ClientUsername\"", value);
        Assert.Contains("x_auth_password=\"ClientPassword\"", value);
    }

    [Fact]
    public void Authenticate_ShouldAddSignatureToRequestAsSeparateParameters_OnUrlOrPostParametersHandling() {
        // Arrange
        const string url = "https://no-query.string";

        var client  = new RestClient(url);
        var request = new RestRequest();
        request.AddQueryParameter("queryparameter", "foobartemp");

        _authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;

        // Act
        _authenticator.Authenticate(client, request);

        // Assert
        var parameters = request.Parameters;

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "x_auth_username" &&
                    (string)x.Value == "ClientUsername" &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "x_auth_password" &&
                    (string)x.Value == "ClientPassword" &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost && x.Name == "x_auth_mode" && (string)x.Value == "client_auth" && x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "oauth_consumer_key" &&
                    (string)x.Value == "ConsumerKey" &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "oauth_signature" &&
                    !string.IsNullOrWhiteSpace((string)x.Value) &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "oauth_signature_method" &&
                    (string)x.Value == "PLAINTEXT" &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_version" && (string)x.Value == "Version" && x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "oauth_nonce" &&
                    !string.IsNullOrWhiteSpace((string)x.Value) &&
                    x.ContentType == null
            )
        );

        Assert.NotNull(
            parameters.FirstOrDefault(
                x => x.Type == ParameterType.GetOrPost &&
                    x.Name == "oauth_timestamp" &&
                    !string.IsNullOrWhiteSpace((string)x.Value) &&
                    x.ContentType == null
            )
        );
    }

    [Theory]
    [InlineData(OAuthType.AccessToken, "Token", "Token")]
    [InlineData(OAuthType.ProtectedResource, "Token", "Token")]
    [InlineData(OAuthType.AccessToken, "SVyDD+RsFzSoZChk=", "SVyDD%2BRsFzSoZChk%3D")]
    [InlineData(OAuthType.ProtectedResource, "SVyDD+RsFzSoZChk=", "SVyDD%2BRsFzSoZChk%3D")]
    public void Authenticate_ShouldEncodeOAuthTokenParameter(OAuthType type, string value, string expected) {
        // Arrange
        const string url = "https://no-query.string";

        var client  = new RestClient(url);
        var request = new RestRequest();
        _authenticator.Type  = type;
        _authenticator.Token = value;

        // Act
        _authenticator.Authenticate(client, request);

        // Assert
        var authParameter = request.Parameters.Single(x => x.Name == KnownHeaders.Authorization);
        var authHeader    = (string)authParameter.Value;

        Assert.NotNull(authHeader);
        Assert.Contains($"oauth_token=\"{expected}\"", authHeader);
    }

    /// <summary>
    /// According the specifications of OAuth 1.0a, the customer secret is not required.
    /// For more information, check the section 4 on https://oauth.net/core/1.0a/.
    /// </summary>
    [Theory]
    [InlineData(OAuthType.AccessToken)]
    [InlineData(OAuthType.ProtectedResource)]
    public void Authenticate_ShouldAllowEmptyConsumerSecret_OnHttpAuthorizationHeaderHandling(OAuthType type) {
        // Arrange
        const string url = "https://no-query.string";

        var client  = new RestClient(url);
        var request = new RestRequest();
        _authenticator.Type           = type;
        _authenticator.ConsumerSecret = null;

        // Act
        _authenticator.Authenticate(client, request);

        // Assert
        var authParameter = request.Parameters.Single(x => x.Name == KnownHeaders.Authorization);
        var value         = (string)authParameter.Value;

        Assert.NotNull(value);
        Assert.NotEmpty(value);
        Assert.Contains("OAuth", value!);
        Assert.Contains($"oauth_signature=\"{OAuthTools.UrlEncodeStrict("&")}", value);
    }
}