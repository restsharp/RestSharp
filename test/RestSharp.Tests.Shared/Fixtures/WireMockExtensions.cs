using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace RestSharp.Tests.Shared.Fixtures;

public static class WireMockExtensions {
    public static RequestBodyCapturer ConfigureBodyCapturer(this WireMockServer server, Method method, bool usePath = true) {
        var capturer = new RequestBodyCapturer();

        var requestBuilder = Request
            .Create()
            .WithPath(usePath ? RequestBodyCapturer.Resource : "/")
            .WithUrl(capturer.CaptureUrl)
            .WithBody(capturer.CaptureBody)
            .WithHeader(capturer.CaptureHeaders)
            .UsingMethod(method.ToString().ToUpper());
        server.Given(requestBuilder).RespondWith(Response.Create().WithStatusCode(200));
        return capturer;
    }
}