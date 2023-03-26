using System.Net;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public class RequestHeadTests : CaptureFixture {
    [Fact]
    public async Task Does_Not_Pass_Default_Credentials_When_Server_Does_Not_Negotiate() {
        const Method httpMethod = Method.Get;

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

        var client = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = true });

        var request = new RestRequest(RequestHeadCapturer.Resource, httpMethod);

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
        const Method httpMethod = Method.Get;

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

        var client   = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = false });
        var request  = new RestRequest(RequestHeadCapturer.Resource, httpMethod);
        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(RequestHeadCapturer.CapturedHeaders);
    }

    [Fact(Skip = "Doesn't work on Linux")]
    public async Task Passes_Default_Credentials_When_UseDefaultCredentials_Is_True() {
        const Method httpMethod = Method.Get;

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>(), AuthenticationSchemes.Negotiate);

        var client   = new RestClient(new RestClientOptions(server.Url) { UseDefaultCredentials = true });
        var request  = new RestRequest(RequestHeadCapturer.Resource, httpMethod);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ToString().Should().BeOneOf(HttpStatusCode.OK.ToString(),HttpStatusCode.Unauthorized.ToString());
        RequestHeadCapturer.CapturedHeaders.Should().NotBeNull();

        var keys = RequestHeadCapturer.CapturedHeaders!.Keys.Cast<string>().ToArray();

        keys.Should()
            .Contain(
                KnownHeaders.Authorization,
                "Authorization header not present in HTTP request from client, even though UseDefaultCredentials = true"
            );
    }
}