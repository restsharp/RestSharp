namespace RestSharp.Authenticators; 

/// <summary>
/// JSON WEB TOKEN (JWT) Authenticator class.
/// <remarks>https://tools.ietf.org/html/draft-ietf-oauth-json-web-token</remarks>
/// </summary>
public class JwtAuthenticator : IAuthenticator {
    string _authHeader;

    // ReSharper disable once IntroduceOptionalParameters.Global
    public JwtAuthenticator(string accessToken) => SetBearerToken(accessToken);

    /// <summary>
    /// Set the new bearer token so the request gets the new header value
    /// </summary>
    /// <param name="accessToken"></param>
    public void SetBearerToken(string accessToken) {
        Ensure.NotEmpty(accessToken, nameof(accessToken));

        _authHeader = $"Bearer {accessToken}";
    }

    public void Authenticate(IRestClient client, IRestRequest request)
        => request.AddOrUpdateParameter("Authorization", _authHeader, ParameterType.HttpHeader);
}