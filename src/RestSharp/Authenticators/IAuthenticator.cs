namespace RestSharp.Authenticators;

public interface IAuthenticator {
    void Authenticate(RestClient client, IRestRequest request);
}