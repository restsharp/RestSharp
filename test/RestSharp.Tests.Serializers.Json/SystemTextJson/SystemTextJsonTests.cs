using RestSharp.Serializers.Json;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Serializers.Json.SystemTextJson;

public sealed class SystemTextJsonTests : IDisposable {
    static readonly Fixture Fixture = new();

    readonly WireMockServer _server = WireMockServer.Start();
    
    [Fact]
    public async Task Use_JsonNet_For_Requests() {
        var serializer = new SystemTextJsonSerializer();
        var capturer   = _server.ConfigureBodyCapturer(Method.Post, false);

        var testData = Fixture.Create<TestClass>();
        var client  = new RestClient(_server.Url!);
        var request = new RestRequest().AddJsonBody(testData);

        await client.PostAsync(request);

        var actual = serializer.Deserialize<TestClass>(new RestResponse(request) { Content = capturer.Body });

        actual.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task Use_JsonNet_For_Response() {
        var expected = Fixture.Create<TestClass>();
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(expected));

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseSystemTextJson());

        var actual = await client.GetAsync<TestClass>(new RestRequest());

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeserilizationFails_IsSuccessful_Should_BeFalse() {
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody("invalid json").WithHeader(KnownHeaders.ContentType, ContentType.Json));

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseSystemTextJson());

        var response = await client.ExecuteAsync<TestClass>(new RestRequest());

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task DeserilizationSucceeds_IsSuccessful_Should_BeTrue() {
        var item = Fixture.Create<TestClass>();
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(item));

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseSystemTextJson());

        var response = await client.ExecuteAsync<TestClass>(new RestRequest());

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeTrue();
    }

    public void Dispose() => _server?.Dispose();
}
