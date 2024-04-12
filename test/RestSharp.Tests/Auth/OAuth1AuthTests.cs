using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace RestSharp.Tests.Auth;

public class OAuth1AuthTests {
    readonly OAuth1Auth _auth = new() {
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

    [Fact]
    public void Authenticate_ShouldAddAuthorizationAsTextValueToRequest_OnHttpAuthorizationHeaderHandling() {
        // Arrange
        const string url = "https://no-query.string";

        using var client  = new RestClient(url);
        var request = new RestRequest();

        _auth.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;

        // Act
        _auth.Authenticate(client, request);

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

        using var client  = new RestClient(url);
        var request = new RestRequest();
        request.AddQueryParameter("queryparameter", "foobartemp");

        _auth.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;

        // Act
        _auth.Authenticate(client, request);

        // Assert
        var parameters = request.Parameters;
        ParameterShouldBe("x_auth_username", "ClientUsername");
        ParameterShouldBe("x_auth_password", "ClientPassword");
        ParameterShouldBe("x_auth_mode", "client_auth");
        ParameterShouldBe("oauth_consumer_key", "ConsumerKey");
        ParameterShouldHaveValue("oauth_signature");
        ParameterShouldBe("oauth_signature_method", "PLAINTEXT");
        ParameterShouldBe("oauth_version", "Version");
        ParameterShouldHaveValue("oauth_nonce");
        ParameterShouldHaveValue("oauth_timestamp");

        void ParameterShould(string name, Func<Parameter, bool> check) {
            var parameter = parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == name);
            parameter.Should().NotBeNull();
            check(parameter).Should().BeTrue();
        }

        void ParameterShouldBe(string name, string value) => ParameterShould(name, x => (string)x.Value == value);

        void ParameterShouldHaveValue(string name) => ParameterShould(name, x => !string.IsNullOrWhiteSpace((string)x.Value));
    }

    [Theory]
    [InlineData(OAuthType.AccessToken, "Token", "Token")]
    [InlineData(OAuthType.ProtectedResource, "Token", "Token")]
    [InlineData(OAuthType.AccessToken, "SVyDD+RsFzSoZChk=", "SVyDD%2BRsFzSoZChk%3D")]
    [InlineData(OAuthType.ProtectedResource, "SVyDD+RsFzSoZChk=", "SVyDD%2BRsFzSoZChk%3D")]
    public void Authenticate_ShouldEncodeOAuthTokenParameter(OAuthType type, string value, string expected) {
        // Arrange
        const string url = "https://no-query.string";

        using var client  = new RestClient(url);
        var request = new RestRequest();
        _auth.Type  = type;
        _auth.Token = value;

        // Act
        _auth.Authenticate(client, request);

        // Assert
        var authParameter = request.Parameters.Single(x => x.Name == KnownHeaders.Authorization);
        var authHeader    = (string)authParameter.Value;

        Assert.NotNull(authHeader);
        Assert.Contains($"oauth_token=\"{expected}\"", authHeader);
    }

    /// <summary>
    /// According to the specifications of OAuth 1.0a, the customer secret is not required.
    /// For more information, check the section 4 on https://oauth.net/core/1.0a/.
    /// </summary>
    [Theory]
    [InlineData(OAuthType.AccessToken)]
    [InlineData(OAuthType.ProtectedResource)]
    public void Authenticate_ShouldAllowEmptyConsumerSecret_OnHttpAuthorizationHeaderHandling(OAuthType type) {
        // Arrange
        const string url = "https://no-query.string";

        using var client  = new RestClient(url);
        var request = new RestRequest();
        _auth.Type           = type;
        _auth.ConsumerSecret = null;

        // Act
        _auth.Authenticate(client, request);

        // Assert
        var authParameter = request.Parameters.Single(x => x.Name == KnownHeaders.Authorization);
        var value         = (string)authParameter.Value;

        Assert.NotNull(value);
        Assert.NotEmpty(value);
        Assert.Contains("OAuth", value!);
        Assert.Contains($"oauth_signature=\"{OAuthTools.UrlEncodeStrict("&")}", value);
    }
}
