using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class AsyncRequestBodyTests : IClassFixture<RequestBodyFixture> {
    public AsyncRequestBodyTests(RequestBodyFixture fixture) {
        var server = fixture.Server;
        _client = new RestClient(server.Url);
    }

    readonly RestClient _client;

    static void AssertHasNoRequestBody() {
        Assert.Null(RequestBodyCapturer.CapturedContentType);
        Assert.False(RequestBodyCapturer.CapturedHasEntityBody);
        Assert.Equal(string.Empty, RequestBodyCapturer.CapturedEntityBody);
    }

    static void AssertHasRequestBody(string contentType, string bodyData) {
        Assert.Equal(contentType, RequestBodyCapturer.CapturedContentType);
        Assert.True(RequestBodyCapturer.CapturedHasEntityBody);
        Assert.Equal(bodyData, RequestBodyCapturer.CapturedEntityBody);
    }

    [Fact]
    public async Task Can_Be_Added_To_COPY_Request() {
        const Method httpMethod = Method.Copy;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_DELETE_Request() {
        const Method httpMethod = Method.Delete;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        var response = await _client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_OPTIONS_Request() {
        const Method httpMethod = Method.Options;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_PATCH_Request() {
        const Method httpMethod = Method.Patch;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_PUT_Request() {
        const Method httpMethod = Method.Put;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        var resetEvent = new ManualResetEvent(false);

        await _client.ExecuteAsync(request);
        resetEvent.WaitOne();

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Have_No_Body_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        var request    = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        await _client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public async Task Can_Be_Added_To_GET_Request() {
        const Method httpMethod = Method.Get;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public async Task Can_Not_Be_Added_To_HEAD_Request() {
        const Method httpMethod = Method.Head;

        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await _client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }
}