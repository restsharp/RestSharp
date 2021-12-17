using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class HttpHeadersTests : CaptureFixture {
    readonly ITestOutputHelper _output;

    public HttpHeadersTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Ensure_headers_correctly_set_in_the_hook() {
        const string headerName  = "HeaderName";
        const string headerValue = "HeaderValue";

        using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

        // Prepare
        var client = new RestClient(server.Url);

        var request = new RestRequest(RequestHeadCapturer.Resource) {
            OnBeforeRequest = http => {
                http.Headers.Add(headerName, headerValue);
                return default;
            }
        };

        // Run
        await client.ExecuteAsync(request);

        // Assert
        RequestHeadCapturer.CapturedHeaders[headerName].Should().Be(headerValue);
    }
}