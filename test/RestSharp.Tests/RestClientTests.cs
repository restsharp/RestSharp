using RestSharp.Serializers;
using RestSharp.Serializers.Json;

namespace RestSharp.Tests;

public class RestClientTests {
    const string BaseUrl = "http://localhost:8888/";

    [Theory]
    [InlineData(Method.Get, Method.Post)]
    [InlineData(Method.Post, Method.Get)]
    [InlineData(Method.Delete, Method.Get)]
    [InlineData(Method.Head, Method.Post)]
    [InlineData(Method.Put, Method.Patch)]
    [InlineData(Method.Patch, Method.Put)]
    [InlineData(Method.Post, Method.Put)]
    [InlineData(Method.Get, Method.Delete)]
    public async Task Execute_with_RestRequest_and_Method_overrides_previous_request_method(Method reqMethod, Method overrideMethod) {
        var req = new RestRequest("", reqMethod);

        using var client = new RestClient(BaseUrl);
        await client.ExecuteAsync(req, overrideMethod);

        req.Method.Should().Be(overrideMethod);
    }

    [Fact]
    public async Task ConfigureHttp_will_set_proxy_to_null_with_no_exceptions_When_no_proxy_can_be_found() {
        var req = new RestRequest();

        using var client = new RestClient(new RestClientOptions(BaseUrl) { Proxy = null });
        await client.ExecuteAsync(req);
    }

    [Fact]
    public void UseJson_leaves_only_json_serializer() {
        // arrange
        var baseUrl = new Uri(BaseUrl);

        // act
        using var client = new RestClient(baseUrl, configureSerialization: cfg => cfg.UseJson());

        // assert
        client.Serializers.Serializers.Should().HaveCount(1);
        client.Serializers.GetSerializer(DataFormat.Json).Should().NotBeNull();
    }

    [Fact]
    public void UseXml_leaves_only_json_serializer() {
        // arrange
        var baseUrl = new Uri(BaseUrl);

        // act
        using var client = new RestClient(baseUrl, configureSerialization: cfg => cfg.UseXml());

        // assert
        client.Serializers.Serializers.Should().HaveCount(1);
        client.Serializers.GetSerializer(DataFormat.Xml).Should().NotBeNull();
    }

    [Fact]
    public void UseOnlySerializer_leaves_only_custom_serializer() {
        // arrange
        var baseUrl = new Uri(BaseUrl);

        // act
        using var client = new RestClient(baseUrl, configureSerialization: cfg => cfg.UseOnlySerializer(() => new SystemTextJsonSerializer()));

        // assert
        client.Serializers.Serializers.Should().HaveCount(1);
        client.Serializers.GetSerializer(DataFormat.Json).Should().NotBeNull();
    }

    [Fact]
    public void Should_reuse_httpClient_instance() {
        using var client1 = new RestClient(new Uri("https://fake.api"), useClientFactory: true);
        using var client2 = new RestClient(new Uri("https://fake.api"), useClientFactory: true);

        client1.HttpClient.Should().BeSameAs(client2.HttpClient);
    }

    [Fact]
    public void Should_use_new_httpClient_instance() {
        using var client1 = new RestClient(new Uri("https://fake.api"));
        using var client2 = new RestClient(new Uri("https://fake.api"));

        client1.HttpClient.Should().NotBeSameAs(client2.HttpClient);
    }

    [Fact]
    public void ConfigureDefaultParameters_sets_user_agent_new_httpClient_instance() {
        // arrange
        var clientOptions = new RestClientOptions();

        // act
        using var restClient = new RestClient(clientOptions);

        //assert
        Assert.Single(
            restClient.DefaultParameters,
            parameter => parameter is { Type: ParameterType.HttpHeader, Name: KnownHeaders.UserAgent, Value: string valueAsString } &&
                valueAsString == clientOptions.UserAgent
        );

        Assert.Empty(restClient.HttpClient.DefaultRequestHeaders.UserAgent);
    }

    [Fact]
    public void ConfigureDefaultParameters_sets_user_agent_given_httpClient_instance() {
        // arrange
        var httpClient    = new HttpClient();
        var clientOptions = new RestClientOptions();

        // act
        using var restClient = new RestClient(httpClient, clientOptions);

        //assert
        Assert.Single(
            restClient.DefaultParameters,
            parameter => parameter is { Type: ParameterType.HttpHeader, Name: KnownHeaders.UserAgent, Value: string valueAsString } &&
                valueAsString == clientOptions.UserAgent
        );

        Assert.Empty(httpClient.DefaultRequestHeaders.UserAgent);
    }
}