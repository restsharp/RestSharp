using RestSharp.Authenticators.OAuth2;

namespace RestSharp.Tests.Integrated.Authentication;

public class OAuth2Tests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    [Fact]
    public async Task ShouldHaveProperHeader() {
        var       auth   = new OAuth2AuthorizationRequestHeaderAuthenticator("token", "Bearer");
        using var client = new RestClient(server.Url!, o => o.Authenticator = auth);

        var response   = await client.GetJsonAsync<TestServerResponse[]>("headers");
        var authHeader = response!.FirstOrDefault(x => x.Name == KnownHeaders.Authorization);

        authHeader.Should().NotBeNull();
        authHeader!.Value.Should().Be("Bearer token");
    }
}