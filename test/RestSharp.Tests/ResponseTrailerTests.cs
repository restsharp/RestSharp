using System.Net;

namespace RestSharp.Tests;

public class ResponseTrailerTests {
#if NET
    [Fact]
    public async Task Should_map_trailing_headers_separately_from_regular_headers() {
        using var client = new RestClient(
            new RestClientOptions("https://dummy.org") { ConfigureMessageHandler = _ => new TrailerResponseHandler() }
        );

        var response = await client.ExecuteAsync(new RestRequest());

        response.TrailingHeaders.Should().BeEquivalentTo(
            new[] {
                new HeaderParameter("Digest", "sha-256=abc"),
                new HeaderParameter("Digest", "sha-256=def"),
                new HeaderParameter("X-Processing-Status", "complete")
            }
        );
        response.Headers.Should().ContainSingle(x => x.Name == "X-Regular-Header" && x.Value == "regular");
        response.Headers.Should().NotContain(x => x.Name == "Digest" || x.Name == "X-Processing-Status");
        response.ContentHeaders.Should().NotContain(x => x.Name == "Digest" || x.Name == "X-Processing-Status");
    }

    class TrailerResponseHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            response.Headers.TryAddWithoutValidation("X-Regular-Header", "regular");
            response.TrailingHeaders.TryAddWithoutValidation("Digest", ["sha-256=abc", "sha-256=def"]);
            response.TrailingHeaders.TryAddWithoutValidation("X-Processing-Status", "complete");
            return Task.FromResult(response);
        }
    }
#endif
}
