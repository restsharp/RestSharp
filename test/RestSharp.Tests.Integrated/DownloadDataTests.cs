using RestSharp.Extensions;

namespace RestSharp.Tests.Integrated;

public sealed class DownloadDataTests : IDisposable {
    const string LocalPath = "Assets/Koala.jpg";

    readonly WireMockServer _server = WireMockServer.Start();
    readonly RestClient     _client;
    readonly string         _path = AppDomain.CurrentDomain.BaseDirectory;

    public DownloadDataTests() {
        var pathToFile = Path.Combine(_path, Path.Combine(LocalPath.Split('/')));

        _server
            .Given(Request.Create().WithPath($"/{LocalPath}"))
            .RespondWith(Response.Create().WithBodyFromFile(pathToFile));
        var options = new RestClientOptions($"{_server.Url}/{LocalPath}") { ThrowOnAnyError = true };
        _client = new(options);
    }

    public void Dispose() => _server.Dispose();

    [Fact]
    public void DownloadDataAsync_returns_null_when_stream_is_null() {
        var request = new RestRequest("/invalid-endpoint");

        var action = () => _client.DownloadData(request);

        action.Should().ThrowExactly<HttpRequestException>();
    }

    [Fact]
    public async Task DownloadDataAsync_returns_bytes_when_stream_has_content() {
        var request = new RestRequest("");

        var bytes = await _client.DownloadDataAsync(request);

        bytes.Should().NotBeNull();
        bytes!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DownloadData_sync_wraps_async() {
        var request = new RestRequest("");

        var bytes = _client.DownloadData(request);

        bytes.Should().NotBeNull();
        bytes!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DownloadStream_sync_wraps_async() {
        var request = new RestRequest("");

        using var stream = _client.DownloadStream(request);

        stream.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadStream_then_ReadAsBytes_matches_DownloadDataAsync() {
        var request = new RestRequest("");

#if NET6_0_OR_GREATER
        await using var stream = await _client.DownloadStreamAsync(request);
#else
        using var stream = await _client.DownloadStreamAsync(request);
#endif
        var bytesFromStream = stream == null ? null : await stream.ReadAsBytes(default);

        var bytesDirect = await _client.DownloadDataAsync(request);

        bytesFromStream.Should().NotBeNull();
        bytesDirect.Should().NotBeNull();
        bytesFromStream!.Should().BeEquivalentTo(bytesDirect);
    }
}