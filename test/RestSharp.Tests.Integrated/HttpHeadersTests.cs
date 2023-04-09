using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated; 

[Collection(nameof(TestServerCollection))]
public class HttpHeadersTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;

    public HttpHeadersTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output = output;
        _client = new RestClient(new RestClientOptions(fixture.Server.Url) { ThrowOnAnyError = true });
    }

    [Fact]
    public async Task Ensure_headers_correctly_set_in_the_hook() {
        const string headerName  = "HeaderName";
        const string headerValue = "HeaderValue";

        var request = new RestRequest("/headers") {
            OnBeforeRequest = http => {
                http.Headers.Add(headerName, headerValue);
                return default;
            }
        };

        // Run
        var response = await _client.ExecuteAsync<TestServerResponse[]>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var header = response.Data!.First(x => x.Name == headerName);
        header.Should().NotBeNull();
        header.Value.Should().Be(headerValue);
    }

    [Fact]
    public async Task Should_use_both_default_and_request_headers() {
        var defaultHeader = new Header("defName", "defValue");
        var requestHeader = new Header("reqName", "reqValue");

        _client.AddDefaultHeader(defaultHeader.Name, defaultHeader.Value);

        var request = new RestRequest("/headers")
            .AddHeader(requestHeader.Name, requestHeader.Value);

        var response = await _client.ExecuteAsync<TestServerResponse[]>(request);
        CheckHeader(defaultHeader);
        CheckHeader(requestHeader);

        void CheckHeader(Header header) {
            var h = response.Data!.First(x => x.Name == header.Name);
            h.Should().NotBeNull();
            h.Value.Should().Be(header.Value);
        }
    }

    record Header(string Name, string Value);
}