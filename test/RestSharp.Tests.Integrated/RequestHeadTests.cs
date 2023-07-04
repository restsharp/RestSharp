using System.Net;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public class RequestHeadTests : CaptureFixture {
    [Fact]
    public async Task Does_Not_Pass_Default_Credentials_When_Server_Does_Not_Negotiate() {
        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

        var client = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = true });

        var request = new RestRequest(RequestHeadCapturer.Resource);

        await client.ExecuteAsync(request);

        Assert.NotNull(RequestHeadCapturer.CapturedHeaders);

        var keys = RequestHeadCapturer.CapturedHeaders.Keys.Cast<string>().ToArray();

        Assert.False(
            keys.Contains(KnownHeaders.Authorization),
            "Authorization header was present in HTTP request from client, even though server does not use the Negotiate scheme"
        );
    }

    [Fact]
    public async Task Does_Not_Pass_Default_Credentials_When_UseDefaultCredentials_Is_False() {
        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

        var client   = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = false });
        var request  = new RestRequest(RequestHeadCapturer.Resource);
        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(RequestHeadCapturer.CapturedHeaders);
    }

    [Fact]
    public async Task Passes_Default_Credentials_When_UseDefaultCredentials_Is_True() {
        if (!OperatingSystem.IsWindows()) return;

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

        var client   = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = true });
        var request  = new RestRequest(RequestHeadCapturer.Resource);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        RequestHeadCapturer.CapturedHeaders.Should().NotBeNull();

        var keys = RequestHeadCapturer.CapturedHeaders!.Keys.Cast<string>().ToArray();

        keys.Should()
            .Contain(
                KnownHeaders.Authorization,
                "Authorization header not present in HTTP request from client, even though UseDefaultCredentials = true"
            );
    }
}
