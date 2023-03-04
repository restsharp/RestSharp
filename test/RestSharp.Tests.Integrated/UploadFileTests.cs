using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class UploadFileTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;
    readonly string            _path = AppDomain.CurrentDomain.BaseDirectory;

    public UploadFileTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output = output;
        _client = new RestClient(new RestClientOptions(fixture.Server.Url) { ThrowOnAnyError = true });
    }

    [Fact]
    public async Task Should_upload_from_file() {
        const string filename = "Koala.jpg";

        var path = Path.Combine(_path, "Assets", filename);

        var request  = new RestRequest("upload").AddFile("file", path);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var expected = new UploadResponse(filename, new FileInfo(path).Length, true);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_upload_from_bytes() {
        const string filename = "Koala.jpg";

        var path  = Path.Combine(_path, "Assets", filename);
        var bytes = await File.ReadAllBytesAsync(path);

        var request  = new RestRequest("upload").AddFile("file", bytes, filename);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        var expected = new UploadResponse(filename, new FileInfo(path).Length, true);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_upload_from_stream() {
        const string filename = "Koala.jpg";

        var path = Path.Combine(_path, "Assets", filename);

        var request  = new RestRequest("upload").AddFile("file", () => File.OpenRead(path), filename);
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        var expected = new UploadResponse(filename, new FileInfo(path).Length, true);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(expected);
    }
}
