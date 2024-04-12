using System.Text;
using System.Web;
using RestSharp.Authenticators;

namespace RestSharp.Tests.Integrated.Authentication;

public class AuthenticationTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    [Fact]
    public async Task Can_Authenticate_With_Basic_Http_Auth() {
        const string userName = "testuser";
        const string password = "testpassword";

        using var client = new RestClient(
            server.Url!,
            o => o.Authenticator = new HttpBasicAuth(userName, password)
        );
        var request  = new RestRequest("headers");
        var response = await client.GetAsync<TestServerResponse[]>(request);

        var header = response!.First(x => x.Name == KnownHeaders.Authorization);
        var auth   = HttpUtility.UrlDecode(header.Value)["Basic ".Length..];
        var value  = Convert.FromBase64String(auth);
        var parts  = Encoding.UTF8.GetString(value).Split(':');

        parts[0].Should().Be(userName);
        parts[1].Should().Be(password);
    }
}