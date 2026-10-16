#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;

namespace RestSharp.Tests.Integrated;

public class ResponseTrailerTests {
    [Fact]
    public async Task Response_trailers_are_preserved_separately_from_headers() {
        using var client = new RestClient(new TrailerResponseHandler());

        var response = await client.ExecuteAsync(new RestRequest("https://example.test/"));

        response.TrailingHeaders.Should().ContainSingle(header => header.Name == "Checksum" && header.Value == "abc123");
        response.Headers.Should().NotContain(header => header.Name == "Checksum");
        response.ContentHeaders.Should().NotContain(header => header.Name == "Checksum");
    }

    sealed class TrailerResponseHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("body")
            };
            response.TrailingHeaders.TryAddWithoutValidation("Checksum", "abc123");
            return Task.FromResult(response);
        }
    }
}
#endif
