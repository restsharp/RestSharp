using System.Net;
using System.Text;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class DownloadFileTests : IDisposable {
    public DownloadFileTests() {
        _server = HttpServerFixture.StartServer("Assets/Koala.jpg", FileHandler);
        _client = new RestClient(_server.Url);
    }

    public void Dispose() => _server.Dispose();

    void FileHandler(HttpListenerRequest request, HttpListenerResponse response) {
        var pathToFile = Path.Combine(
            _path,
            Path.Combine(
                request.Url.Segments.Select(s => s.Replace("/", "")).ToArray()
            )
        );

        using var reader = new StreamReader(pathToFile);

        reader.BaseStream.CopyTo(response.OutputStream);
    }

    readonly HttpServerFixture _server;
    readonly RestClient        _client;
    readonly string            _path = AppDomain.CurrentDomain.BaseDirectory;

    [Fact]
    public async Task AdvancedResponseWriter_without_ResponseWriter_reads_stream() {
        var tag = string.Empty;

        var rr = new RestRequest("Assets/Koala.jpg") {
            AdvancedResponseWriter = response => {
                var buf = new byte[16];
                response.Content.ReadAsStream().Read(buf, 0, buf.Length);
                tag = Encoding.ASCII.GetString(buf, 6, 4);
                return new RestResponse();
            }
        };

        await _client.ExecuteAsync(rr);
        Assert.True(string.Compare("JFIF", tag, StringComparison.Ordinal) == 0);
    }

    [Fact]
    public async Task Handles_Binary_File_Download() {
        var request  = new RestRequest("Assets/Koala.jpg");
        var response = await _client.DownloadDataAsync(request);
        var expected = await File.ReadAllBytesAsync(Path.Combine(_path, "Assets", "Koala.jpg"));

        Assert.Equal(expected, response);
    }

    [Fact]
    public async Task Writes_Response_To_Stream() {
        var tempFile = Path.GetTempFileName();

        var request = new RestRequest("Assets/Koala.jpg") {
            ResponseWriter = responseStream => {
                using var writer = File.OpenWrite(tempFile);

                responseStream.CopyTo(writer);
                return null;
            }
        };
        var response = await _client.DownloadDataAsync(request);

        Assert.Null(response);

        var fromTemp = await File.ReadAllBytesAsync(tempFile);
        var expected = await File.ReadAllBytesAsync(Path.Combine(_path, "Assets", "Koala.jpg"));

        Assert.Equal(expected, fromTemp);
    }
}