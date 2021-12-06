using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class HttpHeadersTests : CaptureFixture {
    [Fact]
    public void Ensure_headers_correctly_set_in_the_hook() {
        const string headerName  = "HeaderName";
        const string headerValue = "HeaderValue";

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

        // Prepare
        var client = new RestClient(server.Url);

        var request = new RestRequest(RequestHeadCapturer.Resource) {
            OnBeforeRequest = http => http.Headers.Add(new HttpHeader(headerName, headerValue))
        };

        // Run
        client.Execute(request);

        // Assert
        RequestHeadCapturer.CapturedHeaders[headerName].Should().Be(headerValue);
    }
}