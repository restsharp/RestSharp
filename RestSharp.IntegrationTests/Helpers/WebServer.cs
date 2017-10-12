using System;
using System.Net;
using System.Text;
using System.Threading;

namespace RestSharp.IntegrationTests
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private Action<HttpListenerContext> _responderMethod;

        public WebServer(string prefix, Action<HttpListenerContext> method, AuthenticationSchemes authenticationSchemes)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException("URI prefix is required");

            _listener.Prefixes.Add(prefix);
            _listener.AuthenticationSchemes = authenticationSchemes;

            _responderMethod = method;
            _listener.Start();
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(c =>
                    {
                        if (!(c is HttpListenerContext ctx)) return;
                        _responderMethod?.Invoke(ctx);
                    }, _listener.GetContext());
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        public void ChangeHandler(Action<HttpListenerContext> handler)
        {
            _responderMethod = handler;
        }
    }
}