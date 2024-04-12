using System.Text;
using RestSharp.Authenticators;

namespace RestSharp.Tests.Auth;

public class HttpBasicAuthTests {
    const string Username = "username";
    const string Password = "password";

    readonly HttpBasicAuth _auth = new(Username, Password);

    [Fact]
    public async Task Authenticate_ShouldAddAuthorizationParameter_IfPreviouslyUnassigned() {
        // Arrange
        using var client = new RestClient();

        var request = new RestRequest();
        request.AddQueryParameter("NotMatching", "", default);
        var expectedToken = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"))}";

        // Act
        await _auth.Authenticate(client, request);

        // Assert
        request.Parameters.Single(x => x.Name == KnownHeaders.Authorization).Value.Should().Be(expectedToken);
    }
}