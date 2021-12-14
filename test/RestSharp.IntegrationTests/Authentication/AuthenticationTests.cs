﻿using System.Net;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests.Authentication; 

public class AuthenticationTests {
    readonly ITestOutputHelper _output;

    public AuthenticationTests(ITestOutputHelper output) => _output = output;

    static void UsernamePasswordEchoHandler(HttpListenerContext context) {
        var header = context.Request.Headers["Authorization"]!;

        var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header["Basic ".Length..]))
            .Split(':');

        context.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
    }

    [Fact]
    public async Task Can_Authenticate_With_Basic_Http_Auth() {
        using var server = SimpleServer.Create(UsernamePasswordEchoHandler);

        var client = new RestClient(server.Url) {
            Authenticator = new HttpBasicAuthenticator("testuser", "testpassword")
        };
        var request  = new RestRequest("test");
        var response = await client.ExecuteAsync(request);

        Assert.Equal("testuser|testpassword", response.Content);
    }
}