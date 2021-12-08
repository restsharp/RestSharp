namespace RestSharp.Authenticators;

public interface IAuthenticator {
    void Authenticate(IRestClient client, IRestRequest request);
}