namespace RestSharp.Authenticators;

public class SimpleAuthenticator : IAuthenticator {
    readonly string _password;
    readonly string _passwordKey;
    readonly string _username;
    readonly string _usernameKey;

    public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password) {
        _usernameKey = usernameKey;
        _username    = username;
        _passwordKey = passwordKey;
        _password    = password;
    }

    public void Authenticate(IRestClient client, IRestRequest request)
        => request
            .AddParameter(_usernameKey, _username)
            .AddParameter(_passwordKey, _password);
}