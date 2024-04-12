using RestSharp.Serializers.Xml;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace RestSharp.Tests.Integrated;

public sealed class StatusCodeTests : IDisposable {
    public StatusCodeTests() {
        _client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseXmlSerializer());
        _server
            .Given(Request.Create())
            .RespondWith(Response.Create().WithCallback(CreateResponse));
        return;

        ResponseMessage CreateResponse(IRequestMessage request) {
            var url = new Uri(request.Url);

            return new ResponseMessage() {
                StatusCode = int.Parse(url.Segments.Last())
            };
        }
    }

    public void Dispose() {
        _server.Dispose();
        _client.Dispose();
    }

    readonly WireMockServer _server = WireMockServer.Start();
    readonly RestClient     _client;

    [Fact]
    public async Task Handles_GET_Request_404_Error() {
        var request  = new RestRequest("404");
        var response = await _client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reports_1xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("100");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_2xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("204");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeTrue();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Reports_3xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("301");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_4xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("404");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_5xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("503");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }
}