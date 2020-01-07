using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Tests.Shared.Fixtures
{
    public class TestHttpServer : IDisposable
    {
        readonly HttpListener          _listener;
        readonly List<TestRequestHandler> _requestHandlers;
        readonly object                _requestHandlersLock = new object();

        public int Port { get; }

        public TestHttpServer(
            int port,
            string url,
            Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>> handlerAction,
            string hostName = "localhost"
        )
            : this(port, new List<TestRequestHandler> {new TestRequestHandler(url, handlerAction)}, hostName) { }

        public TestHttpServer(
            int port,
            List<TestRequestHandler> handlers,
            string hostName = "localhost"
        )
        {
            _requestHandlers = handlers;

            Port = port > 0 ? port : GetRandomUnusedPort();

            //create and start listener
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{hostName}:{Port}/");
            _listener.Start();

// Cannot await Async Call in a Constructor
#pragma warning disable 4014
            HandleRequests();
#pragma warning restore 4014
        }

        static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        async Task HandleRequests()
        {
            try
            {
                //listen for all requests
                while (_listener.IsListening)
                {
                    //get the request
                    var context = await _listener.GetContextAsync();

                    try
                    {
                        Dictionary<string, string> parameters = null;
                        TestRequestHandler            handler;

                        lock (_requestHandlersLock)
                        {
                            handler = _requestHandlers.FirstOrDefault(
                                h => h.TryMatchUrl(context.Request.RawUrl, context.Request.HttpMethod, out parameters)
                            );
                        }

                        string responseString = null;

                        if (handler != null)
                        {
                            //add the query string parameters to the pre-defined url parameters that were set from MatchesUrl()
                            foreach (var qsParamName in context.Request.QueryString.AllKeys)
                                parameters[qsParamName] = context.Request.QueryString[qsParamName];

                            try
                            {
                                handler.HandlerAction(context.Request, context.Response, parameters);
                            }
                            catch (Exception ex)
                            {
                                responseString              = $"Exception in handler: {ex.Message}";
                                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                            }
                        }
                        else
                        {
                            context.Response.ContentType("text/plain").StatusCode(404);
                            responseString = "No handler provided for URL: " + context.Request.RawUrl;
                        }

                        context.Request.ClearContent();

                        //send the response, if there is not (if responseString is null, then the handler method should have manually set the output stream)
                        if (responseString != null)
                        {
                            var buffer = Encoding.UTF8.GetBytes(responseString);
                            context.Response.ContentLength64 += buffer.Length;
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                        context.Response.Close();
                    }
                }
            }
            catch (HttpListenerException ex)
            {
                //when the listener is stopped, it will throw an exception for being cancelled, so just ignore it
                if (ex.Message != "The I/O operation has been aborted because of either a thread exit or an application request")
                    throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (_listener?.IsListening ?? false))
            {
                _listener.Stop();
            }
        }
    }
}