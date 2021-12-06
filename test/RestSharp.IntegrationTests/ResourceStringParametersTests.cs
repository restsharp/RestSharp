using System.Net;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class ResourcestringParametersTests : IDisposable {
    readonly SimpleServer _server;

    public ResourcestringParametersTests() => _server = SimpleServer.Create(RequestHandler.Handle);

    public void Dispose() => _server.Dispose();

    [Fact]
    public void Should_keep_to_parameters_with_the_same_name() {
        const string parameters = "?priority=Low&priority=Medium";

        var client  = new RestClient(_server.Url);
        var request = new RestRequest(parameters);

        client.Get(request);

        var query = RequestHandler.Url?.Query;
        query.Should().Be(parameters);
    }

    static class RequestHandler {
        public static Uri? Url { get; private set; }

        public static void Handle(HttpListenerContext context) {
            Url = context.Request.Url;
            Handlers.Echo(context);
        }
    }
}