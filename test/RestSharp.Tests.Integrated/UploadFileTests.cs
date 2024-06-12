// ReSharper disable MethodHasAsyncOverload

using HttpMultipartParser;
using RestSharp.Extensions;

namespace RestSharp.Tests.Integrated;

public sealed class UploadFileTests : IDisposable {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;
    readonly string            _basePath = AppDomain.CurrentDomain.BaseDirectory;
    readonly string            _path;
    readonly UploadResponse    _expected;
    readonly WireMockServer    _server = WireMockServer.Start();

    const string Filename = "Koala.jpg";

    public UploadFileTests(ITestOutputHelper output) {
        _output   = output;
        _client   = new RestClient(new RestClientOptions(_server.Url!));
        _path     = Path.Combine(_basePath, "Assets", Filename);
        _expected = new UploadResponse(Filename, new FileInfo(_path).Length, true);

        _server
            .Given(Request.Create().WithPath("/upload"))
            .RespondWith(Response.Create().WithCallback(HandleUpload));
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
        var bytes = File.ReadAllBytes(_path);

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

    // This test fails because MultipartFormDataParser doesn't understand filename*
    [Fact]
    public async Task Should_upload_from_stream_non_ascii() {
        const string nonAsciiFilename = "Präsentation_Export.zip";

        var options = new FileParameterOptions { DisableFilenameEncoding = true, DisableFilenameStar = false };

        var request = new RestRequest("upload")
            .AddFile("file", () => File.OpenRead(_path), nonAsciiFilename, options: options)
            .AddQueryParameter("checkFile", "false");
        var response = await _client.ExecutePostAsync<UploadResponse>(request);

        _output.WriteLine(response.Content);
        response.Data.Should().BeEquivalentTo(new UploadResponse(nonAsciiFilename, new FileInfo(_path).Length, true));
    }

    static async Task<ResponseMessage> HandleUpload(IRequestMessage request) {
        var response = new ResponseMessage();

        var checkFile = request.Query == null ||
            request.Query.Count == 0 ||
            request.Query.ContainsKey("checkFile") && bool.Parse(request.Query["checkFile"][0]);

        using var stream = new MemoryStream(request.BodyAsBytes!);
        var       form   = await MultipartFormDataParser.ParseAsync(stream);
        if (form.Files.Count == 0) return response;

        var fileSection = form.Files[0];
        var fileLength  = fileSection.Data.Length;
        var fileName = fileSection.FileName;

        // ReSharper disable once InvertIf
        if (checkFile) {
            var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

            try {
                var expected = File.ReadAllBytes(Path.Combine(assetPath, fileName));
                fileSection.Data.Seek(0, SeekOrigin.Begin);
                var received = await fileSection.Data.ReadAsBytes(default);
                var equal    = received.SequenceEqual(expected);
                return WireMockTestServer.CreateJson(new UploadResponse(fileName, fileLength, equal));
            }
            catch (Exception) {
                return response;
            }
        }

        return WireMockTestServer.CreateJson(new UploadResponse(fileName, fileLength, true));
    }

    public void Dispose() {
        _client.Dispose();
        _server.Dispose();
    }
}