using System.Net;
using HttpTracer;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class MultipartFormDataTests : IDisposable {
    readonly ITestOutputHelper _output;

    public MultipartFormDataTests(ITestOutputHelper output) {
        _output = output;
        _server = SimpleServer.Create(RequestHandler.Handle);

        var options = new RestClientOptions(_server.Url) {
            ConfigureMessageHandler = handler => new HttpTracerHandler(handler, new OutputLogger(output), HttpMessageParts.All)
        };
        _client = new RestClient(options);
    }

    public void Dispose() => _server.Dispose();

    const string LineBreak = "\r\n";

    const string CharsetString            = "charset=utf-8";
    const string ContentTypeString        = $"{KnownHeaders.ContentType}: text/plain; {CharsetString}";
    const string ContentDispositionString = $"{KnownHeaders.ContentDisposition}: form-data;";

    const string Expected =
        $"--{{0}}{LineBreak}{ContentTypeString}{LineBreak}{ContentDispositionString} name=foo{LineBreak}{LineBreak}bar{LineBreak}" +
        $"--{{0}}{LineBreak}{ContentTypeString}{LineBreak}{ContentDispositionString} name=\"a name with spaces\"{LineBreak}{LineBreak}somedata{LineBreak}" +
        $"--{{0}}--{LineBreak}";

    const string ExpectedFileAndBodyRequestContent =
        "--{0}" +
        $"{LineBreak}{KnownHeaders.ContentType}: application/octet-stream" +
        $"{LineBreak}{KnownHeaders.ContentDisposition}: form-data; name=\"fileName\"; filename=\"TestFile.txt\"" +
        $"{LineBreak}{LineBreak}This is a test file for RestSharp.{LineBreak}" +
        $"--{{0}}{LineBreak}{KnownHeaders.ContentType}: application/json; {CharsetString}" +
        $"{LineBreak}{KnownHeaders.ContentDisposition}: form-data; name=controlName" +
        $"{LineBreak}{LineBreak}test{LineBreak}" +
        $"--{{0}}--{LineBreak}";

    const string ExpectedDefaultMultipartContentType = "multipart/form-data; boundary=\"{0}\"";

    const string ExpectedCustomMultipartContentType = "multipart/vnd.resteasy+form-data; boundary=\"{0}\"";

    readonly SimpleServer _server;
    readonly RestClient   _client;

    static class RequestHandler {
        public static string CapturedContentType { get; set; }

        public static void Handle(HttpListenerContext context) {
            CapturedContentType = context.Request.ContentType;
            Handlers.Echo(context);
        }
    }

    static void AddParameters(RestRequest request) {
        request.AddParameter("foo", "bar");
        request.AddParameter("a name with spaces", "somedata");
    }

    [Fact]
    public async Task AlwaysMultipartFormData_WithParameter_Execute() {
        var request = new RestRequest("?json_route=/posts") {
            AlwaysMultipartFormData = true,
            Method                  = Method.Post
        };

        request.AddParameter("title", "test", ParameterType.RequestBody);

        var response = await _client.ExecuteAsync(request);

        Assert.Null(response.ErrorException);
    }

    [Fact]
    public async Task MultipartFormData() {
        var request = new RestRequest("/", Method.Post) {
            AlwaysMultipartFormData = true
        };

        AddParameters(request);

        var response = await _client.ExecuteAsync(request);

        var expected = string.Format(Expected, request.FormBoundary);

        _output.WriteLine($"Expected: {expected}");
        _output.WriteLine($"Actual: {response.Content}");

        Assert.Equal(expected, response.Content);
    }

    [Fact]
    public async Task MultipartFormData_HasDefaultContentType() {
        var request = new RestRequest("/", Method.Post);

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
        request.AddFile("fileName", path);

        request.AddParameter(new BodyParameter("controlName", "test", "application/json"));

        var response = await _client.ExecuteAsync(request);

        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent   = string.Format(ExpectedFileAndBodyRequestContent, boundary);
        var expectedDefaultMultipartContentType = string.Format(ExpectedDefaultMultipartContentType, boundary);

        _output.WriteLine($"Expected: {expectedFileAndBodyRequestContent}");
        _output.WriteLine($"Actual: {response.Content}");

        response.Content.Should().Be(expectedFileAndBodyRequestContent);
        expectedDefaultMultipartContentType.Should().Be(RequestHandler.CapturedContentType);
    }

    [Fact]
    public async Task MultipartFormData_WithCustomContentType() {
        var request = new RestRequest("/", Method.Post);

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");

        const string customContentType = "multipart/vnd.resteasy+form-data";
        request.AddHeader(KnownHeaders.ContentType, customContentType);
        request.AddFile("fileName", path);
        request.AddParameter(new BodyParameter("controlName", "test", "application/json"));

        var response = await _client.ExecuteAsync(request);
        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent  = string.Format(ExpectedFileAndBodyRequestContent, boundary);
        var expectedCustomMultipartContentType = string.Format(ExpectedCustomMultipartContentType, boundary);

        response.Content.Should().Be(expectedFileAndBodyRequestContent);
        RequestHandler.CapturedContentType.Should().Be(expectedCustomMultipartContentType);
    }

    [Fact]
    public async Task MultipartFormData_WithParameterAndFile_Async() {
        var request = new RestRequest("/", Method.Post) {
            AlwaysMultipartFormData = true
        };

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
        request.AddFile("fileName", path);

        request.AddParameter(new BodyParameter("controlName", "test", "application/json"));

        var response = await _client.ExecuteAsync(request);
        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent = string.Format(ExpectedFileAndBodyRequestContent, boundary);

        response.Content.Should().Be(expectedFileAndBodyRequestContent);
    }

    [Fact]
    public async Task MultipartFormDataAsync() {
        var request = new RestRequest("/", Method.Post) { AlwaysMultipartFormData = true };

        AddParameters(request);

        var response = await _client.ExecuteAsync(request);

        var boundary = request.FormBoundary;
        var expected = string.Format(Expected, boundary);

        response.Content.Should().Be(expected);
    }

    [Fact]
    public async Task ShouldHaveJsonContentType() {
        var jsonData = new {
            Company = "Microsoft",
            ZipCode = "LS339",
            Country = "USA"
        };

        var request = new RestRequest {
            Method                  = Method.Post,
            AlwaysMultipartFormData = true
        };
        request.AddJsonBody(jsonData);

        var response = await _client.ExecuteAsync(request);
    }
}