using System.Net;
using RestSharp.Serializers;
using RestSharp.Tests.Shared.Server;
using WireMock.ResponseBuilders;

namespace RestSharp.Tests.Shared;

public abstract class RequestTestsBase(bool disposeClient) {
    protected abstract IRestClient GetClient();

    IRestClient GetTestClient() => new TestClient(GetClient(), disposeClient);

    [Fact]
    public async Task Can_Handle_Exception_Thrown_By_Interceptor_BeforeDeserialization() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var request = new RestRequest("success") {
            Interceptors = [new ThrowingInterceptor(exceptionMessage)]
        };

        using var client   = GetTestClient();
        var       response = await client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact, Obsolete("Obsolete")]
    public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var request = new RestRequest("success");

        request.OnBeforeDeserialization += _ => throw new Exception(exceptionMessage);

        using var client   = GetTestClient();
        var       response = await client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_ExecuteGetAsync_With_Response_Type() {
        using var client   = GetTestClient();
        var       request  = new RestRequest("success");
        var       response = await client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Perform_GET_Async() {
        const string val = "Basic async test";

        var request = new RestRequest($"echo?msg={val}");

        using var client   = GetTestClient();
        var       response = await client.ExecuteAsync(request);
        response.Content.Should().Be(val);
    }

#if NET
    [Fact]
    public async Task Can_Timeout_GET_Async() {
        var request = new RestRequest("timeout").AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = TimeSpan.FromMilliseconds(200);

        using var client   = GetTestClient();
        var       response = await client.ExecuteAsync(request);

        response.ResponseStatus.Should().Be(ResponseStatus.TimedOut, response.ErrorMessage);
    }
#endif

    [Fact]
    public async Task Can_Perform_Delete_With_Response_Type() {
        using var client   = GetTestClient();
        var       request  = new RestRequest("delete");
        var       response = await client.ExecuteAsync<SuccessResponse>(request, Method.Delete);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Delete_With_Response_Type_using_extension() {
        using var client   = GetTestClient();
        var       request  = new RestRequest("delete");
        var       response = await client.DeleteAsync<SuccessResponse>(request);

        response!.Message.Should().Be("Works!");
    }

    class ThrowingInterceptor(string errorMessage) : Interceptors.Interceptor {
        public override ValueTask BeforeDeserialization(RestResponse response, CancellationToken cancellationToken) => throw new(errorMessage);
    }

    class TestClient(IRestClient innerClient, bool disposeClient) : IRestClient {
        public void Dispose() {
            if (disposeClient) innerClient.Dispose();
        }

        public ReadOnlyRestClientOptions Options           => innerClient.Options;
        public RestSerializers           Serializers       => innerClient.Serializers;
        public DefaultParameters         DefaultParameters => innerClient.DefaultParameters;

        public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
            => innerClient.ExecuteAsync(request, cancellationToken);

        public Task<Stream> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default)
            => innerClient.DownloadStreamAsync(request, cancellationToken);
    }
}