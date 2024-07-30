using HttpTracer;
using RestSharp.Tests.Integrated.Fixtures;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class MultipartFormDataTests : IDisposable {
    readonly ITestOutputHelper _output;

    public MultipartFormDataTests(ITestOutputHelper output) {
        _output = output;
        _server = WireMockServer.Start();

        _capturer = _server.ConfigureBodyCapturer(Method.Post);

        var options = new RestClientOptions($"{_server.Url!}{RequestBodyCapturer.Resource}") {
            ConfigureMessageHandler = handler => new HttpTracerHandler(handler, new OutputLogger(output), HttpMessageParts.All)
        };
        _client = new RestClient(options);
    }

    public void Dispose() {
        _server.Dispose();
        _client.Dispose();
    }

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

    readonly WireMockServer      _server;
    readonly RestClient          _client;
    readonly RequestBodyCapturer _capturer;

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
    public async Task MultipartFormData_NoBoundaryQuotes() {
        var request = new RestRequest("/", Method.Post) { AlwaysMultipartFormData = true };

        AddParameters(request);
        request.MultipartFormQuoteBoundary = false;

        await _client.ExecuteAsync(request);

        var expected = string.Format(Expected, request.FormBoundary);

        _capturer.Body.Should().Be(expected);
        _capturer.ContentType.Should().Be($"multipart/form-data; boundary={request.FormBoundary}");
    }

    [Fact]
    public async Task MultipartFormData() {
        var request = new RestRequest("/", Method.Post) { AlwaysMultipartFormData = true };

        AddParameters(request);

        var response = await _client.ExecuteAsync(request);

        var expected = string.Format(Expected, request.FormBoundary);

        _output.WriteLine($"Expected: {expected}");
        _output.WriteLine($"Actual: {response.Content}");

        _capturer.Body.Should().Be(expected);
        _capturer.ContentType.Should().Be($"multipart/form-data; boundary=\"{request.FormBoundary}\"");
    }

    [Fact]
    public async Task MultipartFormData_HasDefaultContentType() {
        var request = new RestRequest("/", Method.Post);

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
        request.AddFile("fileName", path);

        request.AddParameter(new BodyParameter("controlName", "test", ContentType.Json));

        var response = await _client.ExecuteAsync(request);

        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent   = string.Format(ExpectedFileAndBodyRequestContent, boundary);
        var expectedDefaultMultipartContentType = string.Format(ExpectedDefaultMultipartContentType, boundary);

        _output.WriteLine($"Expected: {expectedFileAndBodyRequestContent}");
        _output.WriteLine($"Actual: {response.Content}");

        _capturer.Body.Should().Be(expectedFileAndBodyRequestContent);
        _capturer.ContentType.Should().Be(expectedDefaultMultipartContentType);
    }

    [Fact]
    public async Task MultipartFormData_WithCustomContentType() {
        var request = new RestRequest("/", Method.Post);

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");

        const string customContentType = "multipart/vnd.resteasy+form-data";
        request.AddHeader(KnownHeaders.ContentType, customContentType);
        request.AddFile("fileName", path);
        request.AddParameter(new BodyParameter("controlName", "test", ContentType.Json));

        await _client.ExecuteAsync(request);
        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent  = string.Format(ExpectedFileAndBodyRequestContent, boundary);
        var expectedCustomMultipartContentType = string.Format(ExpectedCustomMultipartContentType, boundary);

        _capturer.Body.Should().Be(expectedFileAndBodyRequestContent);
        _capturer.ContentType.Should().Be(expectedCustomMultipartContentType);
    }

    [Fact]
    public async Task MultipartFormData_WithParameterAndFile_Async() {
        var request = new RestRequest("/", Method.Post) {
            AlwaysMultipartFormData = true
        };

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TestFile.txt");
        request.AddFile("fileName", path);

        request.AddParameter(new BodyParameter("controlName", "test", ContentType.Json));

        await _client.ExecuteAsync(request);
        var boundary = request.FormBoundary;

        var expectedFileAndBodyRequestContent = string.Format(ExpectedFileAndBodyRequestContent, boundary);

        _capturer.Body.Should().Be(expectedFileAndBodyRequestContent);
    }

    [Fact]
    public async Task MultipartFormDataAsync() {
        var request = new RestRequest("/", Method.Post) { AlwaysMultipartFormData = true };

        AddParameters(request);

        await _client.ExecuteAsync(request);

        var boundary = request.FormBoundary;
        var expected = string.Format(Expected, boundary);

        _capturer.Body.Should().Be(expected);
    }

    [Fact]
    public async Task MultipartFormData_Without_File_Creates_A_Valid_RequestBody() {
        using var client = new RestClient(_server.Url!);

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post) {
            AlwaysMultipartFormData = true
        };
        var capturer = _server.ConfigureBodyCapturer(Method.Post);

        const string bodyData      = "abc123 foo bar baz BING!";
        const string multipartName = "mybody";

        request.AddParameter(new BodyParameter(multipartName, bodyData, ContentType.Plain));

        await client.ExecuteAsync(request);

        var expectedBody = new[] {
            ContentTypeString,
            $"{ContentDispositionString} name={multipartName}",
            bodyData
        };

        var actual = capturer.Body!.Replace("\n", string.Empty).Split('\r');
        actual.Should().Contain(expectedBody);
    }

    [Fact]
    public async Task PostParameter_contentType_in_multipart_form() {
        using var client = new RestClient(_server.Url!);

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post) {
            AlwaysMultipartFormData = true
        };
        var capturer = _server.ConfigureBodyCapturer(Method.Post);
        
        const string parameterName = "Arequest";
        const string parameterValue = "{\"attributeFormat\":\"pdf\"}";

        var parameter = new GetOrPostParameter(parameterName, parameterValue) {
            ContentType = "application/json"
        };
        request.AddParameter(parameter);

        await client.ExecuteAsync(request);

        var actual = capturer.Body!.Replace("\n", string.Empty).Split('\r');
        actual[1].Should().Be("Content-Type: application/json; charset=utf-8");
        actual[2].Should().Be($"Content-Disposition: form-data; name={parameterName}");
        actual[4].Should().Be(parameterValue);
    }
}