using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

public class AsyncTests : IDisposable {
    readonly WireMockServer _server = WireMockTestServer.StartTestServer();
    readonly RestClient _client;
    
    public AsyncTests() => _client = new RestClient(_server.Url!);

    [Fact]
    public async Task Can_Handle_Exception_Thrown_By_Interceptor_BeforeDeserialization() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var request = new RestRequest("success") {
            Interceptors = [new ThrowingInterceptor(exceptionMessage)]
        };

        var response = await _client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact, Obsolete("Obsolete")]
    public async Task Can_Handle_Exception_Thrown_By_OnBeforeDeserialization_Handler() {
        const string exceptionMessage = "Thrown from OnBeforeDeserialization";

        var request = new RestRequest("success");

        request.OnBeforeDeserialization += _ => throw new Exception(exceptionMessage);

        var response = await _client.ExecuteAsync<Response>(request);

        Assert.Equal(exceptionMessage, response.ErrorMessage);
        Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_ExecuteGetAsync_With_Response_Type() {
        var request  = new RestRequest("success");
        var response = await _client.ExecuteAsync<SuccessResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Perform_GET_Async() {
        const string val = "Basic async test";

        var request = new RestRequest($"echo?msg={val}");

        var response = await _client.ExecuteAsync(request);
        response.Content.Should().Be(val);
    }

    [Fact]
    public async Task Can_Timeout_GET_Async() {
        var request = new RestRequest("timeout").AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 200;

        var response = await _client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }

    [Fact]
    public async Task Can_Perform_Delete_With_Response_Type() {
        var request  = new RestRequest("delete");
        var response = await _client.ExecuteAsync<SuccessResponse>(request, Method.Delete);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Delete_With_Response_Type_using_extension() {
        var request  = new RestRequest("delete");
        var response = await _client.DeleteAsync<SuccessResponse>(request);

        response!.Message.Should().Be("Works!");
    }

    class ThrowingInterceptor(string errorMessage) : Interceptors.Interceptor {
        public override ValueTask BeforeDeserialization(RestResponse response, CancellationToken cancellationToken)
            => throw new Exception(errorMessage);
    }

    public void Dispose() {
        _server.Dispose();
        _client.Dispose();
    }
}