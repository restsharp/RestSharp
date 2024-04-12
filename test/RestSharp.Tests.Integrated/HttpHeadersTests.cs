namespace RestSharp.Tests.Integrated;

public sealed class HttpHeadersTests(WireMockTestServer server) : IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(new RestClientOptions(server.Url!) { ThrowOnAnyError = true });

    [Fact]
    public async Task Ensure_headers_correctly_set_in_the_interceptor() {
        const string headerName  = "HeaderName";
        const string headerValue = "HeaderValue";

        var request = new RestRequest("/headers") {
            Interceptors = [new HeaderInterceptor(headerName, headerValue)]
        };

        // Run
        var response = await _client.ExecuteAsync<TestServerResponse[]>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var header = response.Data!.First(x => x.Name == headerName);
        header.Should().NotBeNull();
        header.Value.Should().Be(headerValue);
    }

    [Fact, Obsolete("Obsolete")]
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
        response.Data.Should().NotBeNull();
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
        return;

        void CheckHeader(Header header) {
            var h = response.Data!.First(x => x.Name == header.Name);
            h.Should().NotBeNull();
            h.Value.Should().Be(header.Value);
        }
    }

    record Header(string Name, string Value);

    class HeaderInterceptor(string headerName, string headerValue) : Interceptors.Interceptor {
        public override ValueTask BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken) {
            requestMessage.Headers.Add(headerName, headerValue);
            return default;
        }
    }

    public void Dispose() => _client.Dispose();
}