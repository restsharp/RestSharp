using System.Text;
using System.Web;
using RestSharp.Authenticators;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated.Authentication;

[Collection(nameof(TestServerCollection))]
public class AuthenticationTests {
    readonly TestServerFixture _fixture;
    readonly ITestOutputHelper _output;

    public AuthenticationTests(TestServerFixture fixture, ITestOutputHelper output) {
        _fixture = fixture;
        _output  = output;
    }

    [Fact]
    public async Task Can_Authenticate_With_Basic_Http_Auth() {
        const string userName = "testuser";
        const string password = "testpassword";

        var client = new RestClient(
            _fixture.Server.Url,
            o => o.Authenticator = new HttpBasicAuthenticator(userName, password)
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
