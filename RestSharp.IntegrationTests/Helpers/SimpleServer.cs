using System;
using System.Net;

namespace RestSharp.IntegrationTests.Helpers
{
    public class SimpleServer : IDisposable
    {
        private readonly WebServer _server;

        public static SimpleServer Create(string url, Action<HttpListenerContext> handler = null,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous)
        {
            return new SimpleServer(url, handler, authenticationSchemes);
        }

        private SimpleServer(string url, Action<HttpListenerContext> handler = null,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous)
        {
            _server = new WebServer(url, handler, authenticationSchemes);
            _server.Run();
        }

        public void Dispose()
        {
            _server.Stop();
        }

        public void SetHandler(Action<HttpListenerContext> handler)
        {
            _server.ChangeHandler(handler);
        }
    }
}