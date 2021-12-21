using System.Text;
using RestSharp.Authenticators;

namespace RestSharp.Tests; 

public class HttpBasicAuthenticatorTests {
    public HttpBasicAuthenticatorTests() {
        _username = "username";
        _password = "password";

        _authenticator = new HttpBasicAuthenticator(_username, _password);
    }

    readonly string _username;
    readonly string _password;

    readonly HttpBasicAuthenticator _authenticator;

    [Fact]
    public void Authenticate_ShouldAddAuthorizationParameter_IfPreviouslyUnassigned() {
        // Arrange
        var client  = new RestClient();
        var request = new RestRequest();

        request.AddQueryParameter("NotMatching", "", default);

        var expectedToken =
            $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"))}";

        // Act
        _authenticator.Authenticate(client, request);

        // Assert
        request.Parameters.Single(x => x.Name == KnownHeaders.Authorization).Value.Should().Be(expectedToken);
    }
}