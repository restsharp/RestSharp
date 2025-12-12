using System.Net;
using RestSharp.Tests.Fixtures;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests;

public class MultipartFormTests {
    [Fact]
    public async Task ShouldHaveJsonContentType() {
        var jsonData = new {
            Company = "Microsoft",
            ZipCode = "LS339",
            Country = "USA"
        };

        using var client = MockHttpClient.Create(Method.Post, req => req.With(CheckRequest).Respond(HttpStatusCode.OK));

        var request = new RestRequest {
            Method                  = Method.Post,
            AlwaysMultipartFormData = true
        };
        request.AddJsonBody(jsonData);

        var response = await client.ExecuteAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return;

        bool CheckRequest(HttpRequestMessage msg) {
            if (msg.Content is not MultipartFormDataContent formDataContent) return false;

            return formDataContent.First().Headers.ContentType!.MediaType == ContentType.Json;
        }
    }
}
