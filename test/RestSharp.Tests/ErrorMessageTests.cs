using System.Security.Authentication;

namespace RestSharp.Tests;

public class ErrorMessageTests {
    [Fact]
    public async Task ErrorMessage_surfaces_innermost_exception_message() {
        const string innerMessage = "The remote certificate is invalid according to the validation procedure.";

        var innerException = new AuthenticationException(innerMessage);
        var wrappedException = new HttpRequestException("An error occurred while sending the request.", innerException);

        var handler = new FakeHandler(wrappedException);
        var client = new RestClient(new RestClientOptions("https://dummy.org") { ConfigureMessageHandler = _ => handler });

        var response = await client.ExecuteAsync(new RestRequest("/"));

        response.ErrorMessage.Should().Be(innerMessage);
        response.ErrorException.Should().BeOfType<HttpRequestException>();
        response.ErrorException!.InnerException.Should().BeOfType<AuthenticationException>();
        response.ResponseStatus.Should().Be(ResponseStatus.Error);
    }

    [Fact]
    public async Task ErrorMessage_uses_direct_message_when_no_inner_exception() {
        const string message = "No such host is known.";

        var exception = new HttpRequestException(message);
        var handler = new FakeHandler(exception);
        var client = new RestClient(new RestClientOptions("https://dummy.org") { ConfigureMessageHandler = _ => handler });

        var response = await client.ExecuteAsync(new RestRequest("/"));

        response.ErrorMessage.Should().Be(message);
        response.ErrorException.Should().BeOfType<HttpRequestException>();
        response.ErrorException!.InnerException.Should().BeNull();
    }

    class FakeHandler(Exception exception) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw exception;
    }
}
