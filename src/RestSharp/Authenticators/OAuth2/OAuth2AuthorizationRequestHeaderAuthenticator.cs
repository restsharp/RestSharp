namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// The OAuth 2 authenticator using the authorization request header field.
/// </summary>
/// <remarks>
/// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1
/// </remarks>
public class OAuth2AuthorizationRequestHeaderAuthenticator : AuthenticatorBase {
    readonly string _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator" /> class.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken)
        : this(accessToken, "OAuth") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator" /> class.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="tokenType">The token type.</param>
    public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken, string tokenType) : base(accessToken) => _tokenType = tokenType;

    protected override Parameter GetAuthenticationParameter(string accessToken)
        => new("Authorization", $"{_tokenType} {accessToken}", ParameterType.HttpHeader);
}