// ReSharper disable AccessToDisposedClosure

namespace RestSharp.Tests.Integrated.Interceptor;

public class InterceptorTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    [Fact]
    public async Task Should_call_client_interceptor() {
        // Arrange
        var request = CreateRequest();

        var (client, interceptor) = SetupClient(
            test => test.BeforeRequestAction = req => req.AddHeader("foo", "bar")
        );

        //Act
        var response = await client.ExecutePostAsync<TestResponse>(request);

        //Assert
        response.Request.Parameters.Should().Contain(x => x.Name == "foo" && (string)x.Value! == "bar");
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeTrue();
        interceptor.AfterHttpRequestCalled.Should().BeTrue();
        interceptor.AfterRequestCalled.Should().BeTrue();
        interceptor.BeforeDeserializationCalled.Should().BeTrue();

        client.Dispose();
    }

    [Fact]
    public async Task Should_call_request_interceptor() {
        // Arrange
        var request     = CreateRequest();
        var interceptor = new TestInterceptor();
        request.Interceptors = new List<Interceptors.Interceptor> { interceptor };

        //Act
        using var client = new RestClient(server.Url!);
        await client.ExecutePostAsync<TestResponse>(request);

        //Assert
        interceptor.ShouldHaveCalledAll();
    }

    [Fact]
    public async Task Should_call_both_client_and_request_interceptors() {
        // Arrange
        var request = CreateRequest();
        var (client, interceptor) = SetupClient();
        var requestInterceptor = new TestInterceptor();
        request.Interceptors = new List<Interceptors.Interceptor> { requestInterceptor };

        //Act
        await client.ExecutePostAsync<TestResponse>(request);

        //Assert
        interceptor.ShouldHaveCalledAll();
        requestInterceptor.ShouldHaveCalledAll();

        client.Dispose();
    }

    [Fact]
    public async Task ThrowExceptionIn_InterceptBeforeRequest() {
        //Arrange
        var request = CreateRequest();
        var (client, interceptor) = SetupClient(test => test.BeforeRequestAction = req => throw new Exception("DummyException"));

        //Act
        var action = () => client.ExecutePostAsync<TestResponse>(request);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeFalse();
        interceptor.AfterHttpRequestCalled.Should().BeFalse();
        interceptor.AfterRequestCalled.Should().BeFalse();
        interceptor.BeforeDeserializationCalled.Should().BeFalse();

        client.Dispose();
    }

    [Fact]
    public async Task ThrowExceptionIn_InterceptBeforeHttpRequest() {
        // Arrange
        var request = CreateRequest();
        var (client, interceptor) = SetupClient(test => test.BeforeHttpRequestAction = req => throw new Exception("DummyException"));

        //Act
        var action = () => client.ExecutePostAsync<TestResponse>(request);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeTrue();
        interceptor.AfterHttpRequestCalled.Should().BeFalse();
        interceptor.AfterRequestCalled.Should().BeFalse();
        interceptor.BeforeDeserializationCalled.Should().BeFalse();

        client.Dispose();
    }

    [Fact]
    public async Task ThrowException_InInterceptAfterHttpRequest() {
        // Arrange
        var request = CreateRequest();
        var (client, interceptor) = SetupClient(test => test.AfterHttpRequestAction = req => throw new Exception("DummyException"));

        //Act
        var action = () => client.ExecutePostAsync<TestResponse>(request);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeTrue();
        interceptor.AfterHttpRequestCalled.Should().BeTrue();
        interceptor.AfterRequestCalled.Should().BeFalse();
        interceptor.BeforeDeserializationCalled.Should().BeFalse();

        client.Dispose();
    }

    [Fact]
    public async Task ThrowExceptionIn_InterceptAfterRequest() {
        // Arrange
        var request = CreateRequest();
        var (client, interceptor) = SetupClient(test => test.AfterRequestAction = req => throw new Exception("DummyException"));

        //Act
        var action = () => client.ExecutePostAsync<TestResponse>(request);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeTrue();
        interceptor.AfterHttpRequestCalled.Should().BeTrue();
        interceptor.AfterRequestCalled.Should().BeTrue();
        interceptor.BeforeDeserializationCalled.Should().BeFalse();

        client.Dispose();
    }

    (RestClient client, TestInterceptor interceptor) SetupClient(Action<TestInterceptor>? configureInterceptor = null) {
        var interceptor = new TestInterceptor();
        configureInterceptor?.Invoke(interceptor);

        var options = new RestClientOptions(server.Url!) {
            Interceptors = [interceptor]
        };
        return (new RestClient(options), interceptor);
    }

    static RestRequest CreateRequest() {
        var body = new TestRequest("foo", 100);
        return new RestRequest("post/json").AddJsonBody(body);
    }
}

static class InterceptorChecks {
    public static void ShouldHaveCalledAll(this TestInterceptor interceptor) {
        interceptor.BeforeRequestCalled.Should().BeTrue();
        interceptor.BeforeHttpRequestCalled.Should().BeTrue();
        interceptor.AfterHttpRequestCalled.Should().BeTrue();
        interceptor.AfterRequestCalled.Should().BeTrue();
        interceptor.BeforeDeserializationCalled.Should().BeTrue();
    }
}