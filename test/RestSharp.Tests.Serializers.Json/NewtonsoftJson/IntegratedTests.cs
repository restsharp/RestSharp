using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp.Serializers.NewtonsoftJson;

namespace RestSharp.Tests.Serializers.Json.NewtonsoftJson;

public sealed class IntegratedTests : IDisposable {
    static readonly Fixture Fixture = new();

    readonly WireMockServer _server = WireMockServer.Start();

    [Fact, Obsolete("Obsolete")]
    public async Task Use_with_GetJsonAsync() {
        var data       = Fixture.Create<TestClass>();
        var serialized = JsonConvert.SerializeObject(data, JsonNetSerializer.DefaultSettings);

        _server
            .Given(Request.Create().WithPath("/test").UsingGet())
            .RespondWith(Response.Create().WithBody(serialized).WithHeader(KnownHeaders.ContentType, ContentType.Json));

        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseNewtonsoftJson());

        var response = await client.GetAsync<TestClass>("/test");

        response.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task Use_with_GetJsonAsync_custom_settings() {
        var settings = new JsonSerializerSettings {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
        };
        var data       = Fixture.Create<TestClass>();
        var serialized = JsonConvert.SerializeObject(data, settings);

        _server
            .Given(Request.Create().WithPath("/test").UsingGet())
            .RespondWith(Response.Create().WithBody(serialized).WithHeader(KnownHeaders.ContentType, ContentType.Json));

        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseNewtonsoftJson(settings));

        var response = await client.GetAsync<TestClass>("/test");

        response.Should().BeEquivalentTo(data);
    }

    public void Dispose() => _server?.Dispose();
}
