using System.Net;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests.Authentication; 

public class AuthenticationTests {
    static void UsernamePasswordEchoHandler(HttpListenerContext context) {
        var header = context.Request.Headers["Authorization"];

        var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length)))
            .Split(':');

        context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
    }

    [Fact]
    public void Can_Authenticate_With_Basic_Http_Auth() {
        using var server = SimpleServer.Create(UsernamePasswordEchoHandler);

        var client = new RestClient(server.Url) {
            Authenticator = new HttpBasicAuthenticator("testuser", "testpassword")
        };
        var request  = new RestRequest("test");
        var response = client.Execute(request);

        Assert.Equal("testuser|testpassword", response.Content);
    }
}