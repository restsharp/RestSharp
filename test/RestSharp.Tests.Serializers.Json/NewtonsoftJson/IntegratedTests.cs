using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestMockCore;
using RestSharp.Serializers.NewtonsoftJson;

namespace RestSharp.Tests.Serializers.Json.NewtonsoftJson;

public class IntegratedTests {
    static readonly Fixture Fixture = new();

    const int Port = 5001;

    [Fact]
    public async Task Use_with_GetJsonAsync() {
        var data       = Fixture.Create<TestClass>();
        var serialized = JsonConvert.SerializeObject(data, JsonNetSerializer.DefaultSettings);

        using var server = new HttpServer(Port);
        server.Config.Get("/test").Send(serialized);
        server.Run();

        using var client = new RestClient($"http://localhost:{Port}", configureSerialization: cfg => cfg.UseNewtonsoftJson());

        var response = await client.GetJsonAsync<TestClass>("/test");

        response.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task Use_with_GetJsonAsync_custom_settings() {
        var settings = new JsonSerializerSettings {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
        };
        var data       = Fixture.Create<TestClass>();
        var serialized = JsonConvert.SerializeObject(data, settings);

        using var server = new HttpServer(Port);
        server.Config.Get("/test").Send(serialized);
        server.Run();

        using var client = new RestClient($"http://localhost:{Port}", configureSerialization: cfg => cfg.UseNewtonsoftJson(settings));

        var response = await client.GetJsonAsync<TestClass>("/test");

        response.Should().BeEquivalentTo(data);
    }
}
