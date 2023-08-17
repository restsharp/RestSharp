using System.Net;
using System.Text;
using RestSharp.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class DownloadFileTests : IDisposable {
    public DownloadFileTests() {
        _server = HttpServerFixture.StartServer("Assets/Koala.jpg", FileHandler);
        var options = new RestClientOptions(_server.Url) { ThrowOnAnyError = true };
        _client = new RestClient(options);
        _clientNoThrow = new RestClient(_server.Url);
    }

    public void Dispose() => _server.Dispose();

    void FileHandler(HttpListenerRequest request, HttpListenerResponse response) {
        var pathToFile = Path.Combine(
            _path,
            Path.Combine(
                request.Url!.Segments.Select(s => s.Replace("/", "")).ToArray()
            )
        );

        using var reader = new StreamReader(pathToFile);

        reader.BaseStream.CopyTo(response.OutputStream);
    }

    readonly HttpServerFixture _server;
    readonly RestClient        _client;
    readonly RestClient        _clientNoThrow;
    readonly string            _path = AppDomain.CurrentDomain.BaseDirectory;

    [Fact]
    public async Task AdvancedResponseWriter_without_ResponseWriter_reads_stream() {
        var tag = string.Empty;

        // ReSharper disable once UseObjectOrCollectionInitializer
        var rr = new RestRequest("Assets/Koala.jpg");

        rr.AdvancedResponseWriter = (response, request) => {
            var buf = new byte[16];
            // ReSharper disable once MustUseReturnValue
            response.Content.ReadAsStream().Read(buf, 0, buf.Length);
            tag = Encoding.ASCII.GetString(buf, 6, 4);
            return new RestResponse(request);
        };

        await _client.ExecuteAsync(rr);
        Assert.True(string.Compare("JFIF", tag, StringComparison.Ordinal) == 0);
    }

    [Fact]
    public async Task Handles_File_Download_Failure() {
        var request = new RestRequest("Assets/Koala1.jpg");
        var task    = () => _client.DownloadDataAsync(request);
        await task.Should().ThrowAsync<HttpRequestException>().WithMessage("Request failed with status code NotFound");
    }

    [Fact]
    public async Task Handles_Binary_File_Download() {
        var request  = new RestRequest("Assets/Koala.jpg");
        var response = await _client.DownloadDataAsync(request);
        var expected = await File.ReadAllBytesAsync(Path.Combine(_path, "Assets", "Koala.jpg"));

        Assert.Equal(expected, response);
    }

    [Fact]
    public async Task Runs_ErrorHandler_On_Download_Request_Failure() {
        var client = new RestClient("http://localhost:12345");
        var request = new RestRequest("nonexisting");
        RestResponse? errorResponse = null;
        var stream = await client.DownloadStreamAsync(request, (r) => {
            errorResponse = r;
        });

        Assert.Null(stream);
        Assert.NotNull(errorResponse);
        Assert.Equal(ResponseStatus.Error, errorResponse.ResponseStatus);
    }

    [Fact]
    public async Task Runs_ErrorHandler_On_Download_Response_StatusCode_Not_Successful() {
        var request = new RestRequest("Assets/Koala1.jpg");
        RestResponse? errorResponse = null;
        var stream = await _clientNoThrow.DownloadStreamAsync(request, (r) => {
            errorResponse = r;
        });

        Assert.Null(stream);
        Assert.NotNull(errorResponse);
        Assert.Equal(ResponseStatus.Completed, errorResponse.ResponseStatus);
        Assert.False(errorResponse.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Doesnt_Run_ErrorHandler_On_Download_Success() {
        var request = new RestRequest("Assets/Koala.jpg");
        RestResponse? errorResponse = null;
        var stream = await _clientNoThrow.DownloadStreamAsync(request, (r) => {
            errorResponse = r;
        });

        Assert.NotNull(stream);
        Assert.Null(errorResponse);

        var expected = await File.ReadAllBytesAsync(Path.Combine(_path, "Assets", "Koala.jpg"));
        var bytes = await stream.ReadAsBytes(CancellationToken.None);

        Assert.Equal(expected, bytes);
    }

    [Fact]
    public async Task Writes_Response_To_Stream() {
        var tempFile = Path.GetTempFileName();

        // ReSharper disable once UseObjectOrCollectionInitializer
        var request = new RestRequest("Assets/Koala.jpg");

        request.ResponseWriter = responseStream => {
            using var writer = File.OpenWrite(tempFile);

            responseStream.CopyTo(writer);
            return null;
        };
        var response = await _client.DownloadDataAsync(request);

        Assert.Null(response);

        var fromTemp = await File.ReadAllBytesAsync(tempFile);
        var expected = await File.ReadAllBytesAsync(Path.Combine(_path, "Assets", "Koala.jpg"));

        Assert.Equal(expected, fromTemp);
    }
}
