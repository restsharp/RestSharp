namespace RestSharp.Authenticators;

public interface IAuthenticator {
    ValueTask Authenticate(RestClient client, RestRequest request);
}