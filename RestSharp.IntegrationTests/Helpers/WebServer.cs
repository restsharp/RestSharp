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
        private CancellationTokenSource _cts;

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
            _cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(o =>
            {
                var token = (CancellationToken) o;
                while (!token.IsCancellationRequested && _listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(c =>
                    {
                        if (!(c is HttpListenerContext ctx)) return;
                        _responderMethod?.Invoke(ctx);
                        ctx.Response.OutputStream.Close();
                    }, _listener.IsListening ? _listener.GetContext() : null);
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            _listener.Close();
            _cts.Dispose();
        }

        public void ChangeHandler(Action<HttpListenerContext> handler)
        {
            _responderMethod = handler;
        }
    }
}