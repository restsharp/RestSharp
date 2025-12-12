using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated;

public sealed class RequestBodyTests : IDisposable {
    // const string NewLine = "\r\n";

    static readonly string ExpectedTextContentType          = $"{ContentType.Plain}; charset=utf-8";
    static readonly string ExpectedTextContentTypeNoCharset = ContentType.Plain;

    readonly WireMockServer _server = WireMockServer.Start(s => s.AllowBodyForAllHttpMethods = true);

    async Task AssertBody(Method method, bool disableCharset = false) {
#if NET
        var       options  = new RestClientOptions(_server.Url!) { DisableCharset = disableCharset };
        using var client   = new RestClient(options);
        var       request  = new RestRequest(RequestBodyCapturer.Resource, method);
        var       capturer = _server.ConfigureBodyCapturer(method);

        const string bodyData = "abc123 foo bar baz BING!";

        request.AddBody(bodyData, ContentType.Plain);

        await client.ExecuteAsync(request);

        var expected = disableCharset ? ExpectedTextContentTypeNoCharset : ExpectedTextContentType;
        AssertHasRequestBody(capturer, expected, bodyData);
#endif
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
    public Task Can_Be_Added_To_POST_Request_NoCharset() => AssertBody(Method.Post, true);

    [Fact]
    public Task Can_Be_Added_To_POST_Request() => AssertBody(Method.Post);

    [Fact]
    public Task Can_Be_Added_To_PUT_Request_NoCharset() => AssertBody(Method.Put, true);

    [Fact]
    public Task Can_Be_Added_To_PUT_Request() => AssertBody(Method.Put);

    [Fact]
    public async Task Can_Have_No_Body_Added_To_POST_Request() {
        const Method httpMethod = Method.Post;

        using var client   = new RestClient(_server.Url!);
        var       request  = new RestRequest(RequestBodyCapturer.Resource, httpMethod);
        var       capturer = _server.ConfigureBodyCapturer(httpMethod);

        await client.ExecuteAsync(request);

        AssertHasNoRequestBody(capturer);
    }

    [Fact]
    public Task Can_Be_Added_To_GET_Request() => AssertBody(Method.Get);

    [Fact]
    public Task Can_Be_Added_To_HEAD_Request() => AssertBody(Method.Head);

    static void AssertHasNoRequestBody(RequestBodyCapturer capturer) {
        capturer.ContentType.Should().BeNull();
        capturer.HasBody.Should().BeFalse();
        capturer.Body.Should().BeNullOrEmpty();
    }

    static void AssertHasRequestBody(RequestBodyCapturer capturer, string contentType, string bodyData) {
        capturer.ContentType.Should().Be(contentType);
        capturer.HasBody.Should().BeTrue();
        capturer.Body.Should().Be(bodyData);
    }

    public void Dispose() => _server.Dispose();
}