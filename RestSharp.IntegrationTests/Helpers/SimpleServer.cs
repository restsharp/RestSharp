namespace RestSharp.IntegrationTests.Helpers
{
    using System;
    using System.Net;
    using System.Security;
    using System.Threading;

    public class SimpleServer : IDisposable
    {
        private readonly HttpListener _listener;

        private Action<HttpListenerContext> _handler;

        private Thread _thread;

        private SimpleServer(HttpListener listener, Action<HttpListenerContext> handler)
        {
            _listener = listener;
            _handler = handler;
        }

        public static SimpleServer Create(string url, Action<HttpListenerContext> handler = null,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous)
        {
            var listener = new HttpListener
            {
                Prefixes = {url},
                AuthenticationSchemes = authenticationSchemes
            };
            var server = new SimpleServer(listener, handler);

            server.Start();

            return server;
        }

        public void SetHandler(Action<HttpListenerContext> handler) =>
            _handler = handler;

        private void Start()
        {
            if (_listener.IsListening)
            {
                return;
            }

            _listener.Start();

            _thread = new Thread(() =>
            {
                HttpListenerContext context = _listener.GetContext();
                _handler(context);
                context.Response.Close();
            })
            {
                Name = "WebServer"
            };

            _thread.Start();
        }

        public void Dispose()
        {
            try
            {
                _thread.Abort();
            }
            catch (ThreadStateException threadStateException)
            {
                Console.Error.WriteLine("Issue aborting thread - {0}.", threadStateException.Message);
            }
            catch (SecurityException securityException)
            {
                Console.Error.WriteLine("Issue aborting thread - {0}.", securityException.Message);
            }

            if (_listener.IsListening)
            {
                try
                {
                    _listener.Stop();
                }
                catch (ObjectDisposedException objectDisposedException)
                {
                    Console.Error.WriteLine("Issue stopping listener - {0}", objectDisposedException.Message);
                }
            }

            _listener.Close();
        }
    }
}