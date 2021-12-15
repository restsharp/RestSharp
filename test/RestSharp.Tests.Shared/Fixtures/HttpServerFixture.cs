using System.Net;

namespace RestSharp.Tests.Shared.Fixtures; 

public sealed class HttpServerFixture : IDisposable {
    public static HttpServerFixture StartServer(string url, Action<HttpListenerRequest, HttpListenerResponse> handle) {
        var server = new TestHttpServer(0, url, (request, response, _) => handle(request, response));
        return new HttpServerFixture(server);
    }

    public static HttpServerFixture StartServer(Action<HttpListenerRequest, HttpListenerResponse> handle) => StartServer("", handle);

    HttpServerFixture(TestHttpServer server) {
        Url     = $"http://localhost:{server.Port}";
        _server = server;
    }

    public string Url { get; }

    readonly TestHttpServer _server;

    public void Dispose() => _server.Dispose();
}