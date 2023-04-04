using System.Net;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated;

[Collection(nameof(TestServerCollection))]
public class AsyncTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;
    readonly string            _host;

    public AsyncTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output = output;
        _client = new RestClient(fixture.Server.Url);
        _host   = _client.Options.BaseUrl!.Host;
    }

    class Response {
        public string Message { get; set; } = null!;
    }

    [Fact]
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
        var response = await _client.ExecuteAsync<Response>(request);

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
        var response = await _client.ExecuteAsync<Response>(request, Method.Delete);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Can_Delete_With_Response_Type_using_extension() {
        var request  = new RestRequest("delete");
        var response = await _client.DeleteAsync<Response>(request);

        response!.Message.Should().Be("Works!");
    }
}
