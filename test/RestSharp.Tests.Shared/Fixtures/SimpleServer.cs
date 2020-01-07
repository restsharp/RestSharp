using System;
using System.Net;
using NUnit.Framework.Internal;

namespace RestSharp.Tests.Shared.Fixtures
{
    public class SimpleServer : IDisposable
    {
        static readonly Random Random = new Randomizer(DateTimeOffset.Now.Millisecond);

        readonly WebServer _server;
        
        public string Url { get; }
        public string ServerUrl { get; }

        SimpleServer(
            int port,
            Action<HttpListenerContext> handler = null,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous
        )
        {
            Url = $"http://localhost:{port}/";;
            ServerUrl = $"http://{Environment.MachineName}:{port}/";
            _server = new WebServer(Url, handler, authenticationSchemes);
            _server.Run();
        }

        public void Dispose() => _server.Stop();

        public static SimpleServer Create(
            Action<HttpListenerContext> handler = null,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous
        )
        {
            var port = Random.Next(1000, 9999);
            return new SimpleServer(port, handler, authenticationSchemes);
        }

        public void SetHandler(Action<HttpListenerContext> handler) => _server.ChangeHandler(handler);
    }
}