// ReSharper disable ClassNeverInstantiated.Local

namespace RestSharp.Tests.Integrated;

public sealed class RequestFailureTests(WireMockTestServer server) : IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(server.Url!);

    [Fact]
    public async Task Handles_GET_Request_Errors() {
        var request  = new RestRequest("status?code=404");
        var response = await _client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handles_GET_Request_Errors_With_Response_Type() {
        var request  = new RestRequest("status?code=404");
        var response = await _client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.IsSuccessStatusCode.Should().BeFalse();
        response.ErrorException.Should().NotBeNull();
        response.Data.Should().Be(null);
    }

    [Fact]
    public async Task Does_not_throw_on_unsuccessful_status_code_with_option() {
        using var client   = new RestClient(new RestClientOptions(server.Url!) { SetErrorExceptionOnUnsuccessfulStatusCode = false });
        var       request  = new RestRequest("status?code=404");
        var       response = await client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.IsSuccessStatusCode.Should().BeFalse();
        response.ErrorException.Should().BeNull();
        response.Data.Should().Be(null);
    }

    [Fact]
    public async Task Throws_on_unsuccessful_call() {
        using var client  = new RestClient(new RestClientOptions(server.Url!) { ThrowOnAnyError = true });
        var       request = new RestRequest("status?code=500");

        // ReSharper disable once AccessToDisposedClosure
        var task = () => client.ExecuteAsync<SuccessResponse>(request);
        await task.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetAsync_throws_on_unsuccessful_call() {
        var request = new RestRequest("status?code=500");

        var task = () => _client.GetAsync(request);
        await task.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetAsync_completes_on_404() {
        var request = new RestRequest("status?code=404");

        var response = await _client.GetAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ResponseStatus.Should().Be(ResponseStatus.Completed);
    }

    [Fact]
    public async Task GetAsync_generic_throws_on_unsuccessful_call() {
        var request = new RestRequest("status?code=500");

        var task = () => _client.GetAsync<SuccessResponse>(request);
        await task.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetAsync_returns_null_on_404() {
        var request = new RestRequest("status?code=404");

        var response = await _client.GetAsync<SuccessResponse>(request);
        response.Should().BeNull();
    }

    public void Dispose() => _client.Dispose();
}