namespace RestSharp.Authenticators; 

public abstract class AuthenticatorBase : IAuthenticator {
    protected AuthenticatorBase(string token) => Token = token;

    protected string Token { get; }

    protected abstract Parameter GetAuthenticationParameter(string accessToken);

    public void Authenticate(IRestClient client, IRestRequest request) => request.AddOrUpdateParameter(GetAuthenticationParameter(Token));
}