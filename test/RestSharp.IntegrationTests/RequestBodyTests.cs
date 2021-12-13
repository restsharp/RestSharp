using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class RequestBodyTests : IClassFixture<RequestBodyFixture> {
    readonly SimpleServer _server;

    const string NewLine = "\r\n";

    const string TextPlainContentType    = "text/plain";
    const string ExpectedTextContentType = $"{TextPlainContentType}; charset=utf-8";

    public RequestBodyTests(RequestBodyFixture fixture) => _server = fixture.Server;

    async Task AssertBody(Method method) {
        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, method);

        const string bodyData = "abc123 foo bar baz BING!";

        request.AddParameter(TextPlainContentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(ExpectedTextContentType, bodyData);
    }

    [Fact]
    public Task Can_Be_Added_To_COPY_Request() => AssertBody(Method.Copy);

    [Fact]
    public Task Can_Be_Added_To_DELETE_Request() => AssertBody(Method.Delete);

    [Fact]
    public Task Can_Be_Added_To_OPTIONS_Request() => AssertBody(Method.Options);

    [Fact]
    public Task Can_Be_Added_To_PATCH_Request() => AssertBody(Method.Patch);

    [Fact]
    public Task Can_Be_Added_To_POST_Request() => AssertBody(Method.Post);

    [Fact]
    public Task Can_Be_Added_To_PUT_Request() => AssertBody(Method.Put);

    [Fact]
    public async Task Can_Have_No_Body_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        await client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public Task Can_Be_Added_To_GET_Request() => AssertBody(Method.Get);

    [Fact]
    public Task Can_Be_Added_To_HEAD_Request() => AssertBody(Method.Head);

    [Fact]
    public async Task MultipartFormData_Without_File_Creates_A_Valid_RequestBody() {
        var client = new RestClient(_server.Url);

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post) {
            AlwaysMultipartFormData = true
        };

        const string bodyData      = "abc123 foo bar baz BING!";
        const string multipartName = "mybody";

        request.AddParameter(multipartName, bodyData, TextPlainContentType, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        var expectedBody = new[] {
            $"{KnownHeaders.ContentType}: {ExpectedTextContentType}",
            $"{KnownHeaders.ContentDisposition}: form-data; name={multipartName}",
            bodyData
        };

        var actual = RequestBodyCapturer.CapturedEntityBody.Split(NewLine);
        actual.Should().Contain(expectedBody);
    }

    [Fact]
    public async Task Query_Parameters_With_Json_Body() {
        const Method httpMethod = Method.Put;

        var client = new RestClient(_server.Url);

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod)
            .AddJsonBody(new { displayName = "Display Name" })
            .AddQueryParameter("key", "value");

        await client.ExecuteAsync(request);

        Assert.Equal($"{_server.Url}Capture?key=value", RequestBodyCapturer.CapturedUrl.ToString());
        Assert.Equal("application/json; charset=utf-8", RequestBodyCapturer.CapturedContentType);
        Assert.Equal("{\"displayName\":\"Display Name\"}", RequestBodyCapturer.CapturedEntityBody);
    }

    static void AssertHasNoRequestBody() {
        RequestBodyCapturer.CapturedContentType.Should().BeNull();
        RequestBodyCapturer.CapturedHasEntityBody.Should().BeFalse();
        RequestBodyCapturer.CapturedEntityBody.Should().BeNullOrEmpty();
    }

    static void AssertHasRequestBody(string contentType, string bodyData) {
        RequestBodyCapturer.CapturedContentType.Should().Be(contentType);
        RequestBodyCapturer.CapturedHasEntityBody.Should().BeTrue();
        RequestBodyCapturer.CapturedEntityBody.Should().Be(bodyData);
    }
}