using System.Net;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests;

public class MultipartFormTests {
    [Fact]
    public async Task ShouldHaveJsonContentType() {
        const string url = "https://dummy.org";

        var jsonData = new {
            Company = "Microsoft",
            ZipCode = "LS339",
            Country = "USA"
        };

        var mockHttp = new MockHttpMessageHandler();

        mockHttp
            .When(HttpMethod.Post, url)
            .With(CheckRequest)
            .Respond(HttpStatusCode.OK);

        var options = new RestClientOptions(url) {
            ConfigureMessageHandler = h => mockHttp
        };
        var client = new RestClient(options);

        var request = new RestRequest {
            Method                  = Method.Post,
            AlwaysMultipartFormData = true
        };
        request.AddJsonBody(jsonData);

        var response = await client.ExecuteAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        bool CheckRequest(HttpRequestMessage msg) {
            if (msg.Content is not MultipartFormDataContent formDataContent) return false;

            return formDataContent.First().Headers.ContentType!.MediaType == "application/json";
        }
    }
}
