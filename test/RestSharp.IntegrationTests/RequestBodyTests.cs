using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class RequestBodyTests : IClassFixture<RequestBodyFixture> {
    readonly SimpleServer _server;

    const string NewLine = "\r\n";

    public RequestBodyTests(RequestBodyFixture fixture) => _server = fixture.Server;

    [Fact]
    public async Task Can_Be_Added_To_COPY_Request() {
        const Method httpMethod = Method.Copy;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_DELETE_Request() {
        const Method httpMethod = Method.Delete;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_OPTIONS_Request() {
        const Method httpMethod = Method.Options;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_PATCH_Request() {
        const Method httpMethod = Method.Patch;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Be_Added_To_PUT_Request() {
        const Method httpMethod = Method.Put;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasRequestBody(contentType, bodyData);
    }

    [Fact]
    public async Task Can_Have_No_Body_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        await client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public async Task Can_Not_Be_Added_To_GET_Request() {
        const Method httpMethod = Method.Get;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public async Task Can_Not_Be_Added_To_HEAD_Request() {
        const Method httpMethod = Method.Head;

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(RequestBodyCapturer.Resource, httpMethod);

        const string contentType = "text/plain";
        const string bodyData    = "abc123 foo bar baz BING!";

        request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        AssertHasNoRequestBody();
    }

    [Fact]
    public async Task MultipartFormData_Without_File_Creates_A_Valid_RequestBody() {
        string? expectedFormBoundary = null;

        var client = new RestClient(_server.Url);

        var request = new RestRequest(RequestBodyCapturer.Resource, Method.Post) {
            AlwaysMultipartFormData = true
        };
        request.OnBeforeRequest += http => expectedFormBoundary = http.FormBoundary;

        const string contentType   = "text/plain";
        const string bodyData      = "abc123 foo bar baz BING!";
        const string multipartName = "mybody";

        request.AddParameter(multipartName, bodyData, contentType, ParameterType.RequestBody);

        await client.ExecuteAsync(request);

        var expectedBody = "--" +
            expectedFormBoundary +
            NewLine +
            "Content-Type: " +
            contentType +
            NewLine +
            @"Content-Disposition: form-data; name=""" +
            multipartName +
            @"""" +
            NewLine +
            NewLine +
            bodyData +
            NewLine +
            "--" +
            expectedFormBoundary +
            "--" +
            NewLine;

        Assert.Equal(expectedBody, RequestBodyCapturer.CapturedEntityBody);
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
        Assert.Equal("application/json", RequestBodyCapturer.CapturedContentType);
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