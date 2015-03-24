namespace RestSharp.IntegrationTests.Helpers
{
    using System;
    using System.Net;
    using System.Threading;

    public class SimpleServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Action<HttpListenerContext> handler;
        private Thread processor;

        public static SimpleServer Create(
            string url,
            Action<HttpListenerContext> handler,
            AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous)
        {
            var listener = new HttpListener { Prefixes = { url }, AuthenticationSchemes = authenticationSchemes };
            var server = new SimpleServer(listener, handler);

            server.Start();
            return server;
        }

        private SimpleServer(HttpListener listener, Action<HttpListenerContext> handler)
        {
            this.listener = listener;
            this.handler = handler;
        }

        public void Start()
        {
            if (this.listener.IsListening)
            {
                return;
            }

            this.listener.Start();

            this.processor = new Thread(() =>
            {
                var context = this.listener.GetContext();
                this.handler(context);
                context.Response.Close();
            }) { Name = "WebServer" };

            this.processor.Start();
        }

        public void Dispose()
        {
            this.processor.Abort();
            this.listener.Stop();
            this.listener.Close();
        }
    }
}
