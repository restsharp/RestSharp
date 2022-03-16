using RestSharp.Authenticators.OAuth2;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated.Authentication; 

[Collection(nameof(TestServerCollection))]
public class OAuth2Tests {
    readonly TestServerFixture _fixture;

    public OAuth2Tests(TestServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ShouldHaveProperHeader() {
        var client = new RestClient(_fixture.Server.Url);
        var auth   = new OAuth2AuthorizationRequestHeaderAuthenticator("token", "Bearer");

        var response   = await client.GetJsonAsync<TestServerResponse[]>("headers", authenticator: auth);
        var authHeader = response!.FirstOrDefault(x => x.Name == KnownHeaders.Authorization);

        authHeader.Should().NotBeNull();
        authHeader!.Value.Should().Be("Bearer token");
    }
}