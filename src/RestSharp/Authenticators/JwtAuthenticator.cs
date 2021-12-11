namespace RestSharp.Authenticators; 

/// <summary>
/// JSON WEB TOKEN (JWT) Authenticator class.
/// <remarks>https://tools.ietf.org/html/draft-ietf-oauth-json-web-token</remarks>
/// </summary>
public class JwtAuthenticator : IAuthenticator {
    string _authHeader = null!;

    // ReSharper disable once IntroduceOptionalParameters.Global
    public JwtAuthenticator(string accessToken) => SetBearerToken(accessToken);

    /// <summary>
    /// Set the new bearer token so the request gets the new header value
    /// </summary>
    /// <param name="accessToken"></param>
    [PublicAPI]
    public void SetBearerToken(string accessToken) => _authHeader = $"Bearer {Ensure.NotEmpty(accessToken, nameof(accessToken))}";

    public ValueTask Authenticate(RestClient client, IRestRequest request) {
        request.AddOrUpdateParameter("Authorization", _authHeader, ParameterType.HttpHeader);
        return default;
    }
}