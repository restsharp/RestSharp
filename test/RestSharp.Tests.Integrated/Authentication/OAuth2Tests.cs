using RestSharp.Authenticators.OAuth2;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated.Authentication;

public class OAuth2Tests : IDisposable {
    readonly WireMockServer _server = WireMockTestServer.StartTestServer();
    
    [Fact]
    public async Task ShouldHaveProperHeader() {
        var auth   = new OAuth2AuthorizationRequestHeaderAuthenticator("token", "Bearer");
        var client = new RestClient(_server.Url!, o => o.Authenticator = auth);

        var response   = await client.GetJsonAsync<TestServerResponse[]>("headers");
        var authHeader = response!.FirstOrDefault(x => x.Name == KnownHeaders.Authorization);

        authHeader.Should().NotBeNull();
        authHeader!.Value.Should().Be("Bearer token");
    }

    public void Dispose() => _server.Dispose();
}
