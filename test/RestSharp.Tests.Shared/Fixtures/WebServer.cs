using System;
using System.Net;
using System.Threading;

namespace RestSharp.Tests.Shared.Fixtures
{
    public class WebServer
    {
        readonly HttpListener       _listener = new HttpListener();
        CancellationTokenSource     _cts;
        Action<HttpListenerContext> _responderMethod;

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

            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    var token = (CancellationToken) o;

                    try
                    {
                        while (!token.IsCancellationRequested && _listener.IsListening)
                        {
                            ThreadPool.QueueUserWorkItem(
                                c =>
                                {
                                    try
                                    {
                                        if (!(c is HttpListenerContext ctx)) return;

                                        _responderMethod?.Invoke(ctx);
                                        ctx.Response.OutputStream?.Close();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                }, _listener.IsListening ? _listener.GetContext() : null
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }, _cts.Token
            );
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            _listener.Close();
            _cts.Dispose();
        }

        public void ChangeHandler(Action<HttpListenerContext> handler) => _responderMethod = handler;
    }
}