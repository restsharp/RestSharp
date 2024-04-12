using System.Text;
// ReSharper disable MethodHasAsyncOverload

namespace RestSharp.Tests.Integrated;

public sealed class DownloadFileTests : IDisposable {
    const string LocalPath = "Assets/Koala.jpg";

    public DownloadFileTests() {
        // _server = HttpServerFixture.StartServer("Assets/Koala.jpg", FileHandler);

        var pathToFile = Path.Combine(_path, Path.Combine(LocalPath.Split('/')));

        _server
            .Given(Request.Create().WithPath($"/{LocalPath}"))
            .RespondWith(Response.Create().WithBodyFromFile(pathToFile));
        var options = new RestClientOptions($"{_server.Url}/{LocalPath}") { ThrowOnAnyError = true };
        _client = new RestClient(options);
    }

    public void Dispose() => _server.Dispose();

    readonly WireMockServer _server = WireMockServer.Start();
    readonly RestClient     _client;
    readonly string         _path = AppDomain.CurrentDomain.BaseDirectory;

    [Fact]
    public async Task AdvancedResponseWriter_without_ResponseWriter_reads_stream() {
        var tag = string.Empty;

        var rr = new RestRequest("") {
            AdvancedResponseWriter = (response, request) => {
                var buf = new byte[16];
                // ReSharper disable once MustUseReturnValue
                response.Content.ReadAsStreamAsync().GetAwaiter().GetResult().Read(buf, 0, buf.Length);
                tag = Encoding.ASCII.GetString(buf, 6, 4);
                return new RestResponse(request);
            }
        };

        await _client.ExecuteAsync(rr);
        Assert.Equal(0, string.Compare("JFIF", tag, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handles_File_Download_Failure() {
        var request = new RestRequest("some/other/path");
        var task    = () => _client.DownloadDataAsync(request);
        await task.Should().ThrowAsync<HttpRequestException>().WithMessage("Request failed with status code NotFound");
    }

    [Fact]
    public async Task Handles_Binary_File_Download() {
        var request  = new RestRequest("");
        var response = await _client.DownloadDataAsync(request);
        var expected = File.ReadAllBytes(Path.Combine(_path, Path.Combine(LocalPath.Split('/'))));

        Assert.Equal(expected, response);
    }

    [Fact]
    public async Task Writes_Response_To_Stream() {
        var tempFile = Path.GetTempFileName();

        var request = new RestRequest("") {
            ResponseWriter = responseStream => {
                using var writer = File.OpenWrite(tempFile);
                responseStream.CopyTo(writer);
                return null;
            }
        };

        var response = await _client.DownloadDataAsync(request);

        Assert.Null(response);

        var fromTemp = File.ReadAllBytes(tempFile);
        var expected = File.ReadAllBytes(Path.Combine(_path, Path.Combine(LocalPath.Split('/'))));

        Assert.Equal(expected, fromTemp);
    }
}