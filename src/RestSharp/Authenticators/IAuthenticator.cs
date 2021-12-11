namespace RestSharp.Authenticators;

public interface IAuthenticator {
    ValueTask Authenticate(RestClient client, IRestRequest request);
}