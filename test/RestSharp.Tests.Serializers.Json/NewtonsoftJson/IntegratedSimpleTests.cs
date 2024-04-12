using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Serializers.Json.NewtonsoftJson;

public sealed class IntegratedSimpleTests : IDisposable {
    static readonly Fixture Fixture = new();

    readonly WireMockServer _server = WireMockServer.Start();
    readonly RestClient     _client;

    public IntegratedSimpleTests() => _client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseNewtonsoftJson());

    [Fact]
    public async Task Should_serialize_request() {
        var capturer   = _server.ConfigureBodyCapturer(Method.Post, false);
        var serializer = new JsonNetSerializer();
        var testData   = Fixture.Create<TestClass>();
        var request    = new RestRequest().AddJsonBody(testData);

        await _client.PostAsync(request);
        var actual = serializer.Deserialize<TestClass>(new RestResponse(request) { Content = capturer.Body! });

        actual.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task Should_deserialize_response() {
        var expected = Fixture.Create<TestClass>();

        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(expected));

        var actual = await _client.GetAsync<TestClass>(new RestRequest());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Invalid_json_body_request_should_fail() {
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody("invalid json").WithHeader(KnownHeaders.ContentType, ContentType.Json));


        var response = await _client.ExecuteAsync<TestClass>(new RestRequest());
        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task Valid_json_response_should_succeed() {
        var item = Fixture.Create<TestClass>();

        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(item));

        var response = await _client.ExecuteAsync<TestClass>(new RestRequest());
        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeTrue();
    }

    public void Dispose() {
        _server?.Dispose();
        _client.Dispose();
    }
}