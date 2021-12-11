namespace RestSharp.Authenticators;

public abstract class AuthenticatorBase : IAuthenticator {
    protected AuthenticatorBase(string token) => Token = token;

    protected string Token { get; }

    protected abstract ValueTask<Parameter> GetAuthenticationParameter(string accessToken);

    public async ValueTask Authenticate(RestClient client, IRestRequest request)
        => request.AddOrUpdateParameter(await GetAuthenticationParameter(Token));
}