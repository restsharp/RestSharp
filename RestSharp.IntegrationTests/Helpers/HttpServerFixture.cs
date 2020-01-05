using System;
using System.Net;
using MockHttpServer;

namespace RestSharp.IntegrationTests.Helpers
{
    public class HttpServerFixture : IDisposable
    {
        public static HttpServerFixture StartServer(string url, Action<HttpListenerRequest, HttpListenerResponse> handle)
        {
            var server = new MockServer(0, url, (request, response, _) => handle(request, response));
            return new HttpServerFixture(server);
        }

        public static HttpServerFixture StartServer(Action<HttpListenerRequest, HttpListenerResponse> handle) => StartServer("", handle);

        HttpServerFixture(MockServer server)
        {
            Url     = $"http://localhost:{server.Port}";
            _server = server;
        }

        public string Url { get; }

        MockServer _server;
        public void Dispose() => _server.Dispose();
    }
}