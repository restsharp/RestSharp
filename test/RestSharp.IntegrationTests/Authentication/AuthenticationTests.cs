using System.Net;
using System.Text;
using System.Web;
using RestSharp.Authenticators;
using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests.Authentication;

public class AuthenticationTests {
    readonly ITestOutputHelper _output;

    public AuthenticationTests(ITestOutputHelper output) => _output = output;

    static void UsernamePasswordEchoHandler(HttpListenerContext context) {
        var header = context.Request.Headers["Authorization"]!;

        var parts = Encoding.ASCII
            .GetString(Convert.FromBase64String(header["Basic ".Length..]))
            .Split(':');

        context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
    }

    [Fact]
    public async Task Can_Authenticate_With_Basic_Http_Auth() {
        const string userName = "testuser";
        const string password = "testpassword";

        var server = new HttpServer();
        await server.Start();

        var client = new RestClient(server.Url) {
            Authenticator = new HttpBasicAuthenticator(userName, password)
        };
        var request  = new RestRequest("headers");
        var response = await client.GetAsync<TestServerResponse[]>(request);

        var header = response!.First(x => x.Name == KnownHeaders.Authorization);
        var auth   = HttpUtility.UrlDecode(header.Value)["Basic ".Length..];
        var value  = Convert.FromBase64String(auth);
        var parts  = Encoding.UTF8.GetString(value).Split(':');
        
        parts[0].Should().Be(userName);
        parts[1].Should().Be(password);

        await server.Stop();
    }
}