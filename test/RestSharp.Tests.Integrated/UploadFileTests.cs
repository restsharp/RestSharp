using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class UploadFileTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;
    readonly string            _basePath = AppDomain.CurrentDomain.BaseDirectory;
    readonly string            _path;
    readonly UploadResponse    _expected;

    const string Filename = "Koala.jpg";

    public UploadFileTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output = output;
        // _client = new RestClient(new RestClientOptions(fixture.Server.Url) { ThrowOnAnyError = true });
        _client   = new RestClient(new RestClientOptions(fixture.Server.Url) { ThrowOnAnyError = false });
        _path     = Path.Combine(_basePath, "Assets", Filename);
        _expected = new UploadResponse(Filename, new FileInfo(_path).Length, true);
    }

    [Fact]
    public async Task Should_upload_from_file() {
        var request  = new RestRequest("upload").AddFile("file", _path);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(_expected);
    }

    [Fact]
    public async Task Should_upload_from_bytes() {
        var bytes = await File.ReadAllBytesAsync(_path);

        var request  = new RestRequest("upload").AddFile("file", bytes, Filename);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(_expected);
    }

    [Fact]
    public async Task Should_upload_from_stream() {
        var request  = new RestRequest("upload").AddFile("file", () => File.OpenRead(_path), Filename);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(_expected);
    }

    [Fact]
    public async Task Should_upload_from_stream_non_ascii() {
        const string nonAsciiFilename = "Präsentation_Export.zip";

        var options = new FileParameterOptions { DisableFilenameEncoding = true, DisableFilenameStar = false};

        var request = new RestRequest("upload")
            .AddFile("file", () => File.OpenRead(_path), nonAsciiFilename, options: options)
            .AddQueryParameter("checkFile", "false");
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(new UploadResponse(nonAsciiFilename, new FileInfo(_path).Length, true));
    }
}
