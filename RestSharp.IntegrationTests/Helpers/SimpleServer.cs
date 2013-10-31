using System;
using System.Net;
using System.Threading;

namespace RestSharp.IntegrationTests.Helpers
{
	public class SimpleServer : IDisposable
	{
		readonly HttpListener _listener;
		readonly Action<HttpListenerContext> _handler;
		Thread _processor;

		public static SimpleServer Create(string url, Action<HttpListenerContext> handler, AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.Anonymous)
		{
			var listener = new HttpListener
			{
				Prefixes = { url },
				AuthenticationSchemes = authenticationSchemes
			};
			var server = new SimpleServer(listener, handler);
			server.Start();
			return server;
		}

		SimpleServer(HttpListener listener, Action<HttpListenerContext> handler)
		{
			_listener = listener;
			_handler = handler;
		}

		public void Start()
		{
			if(!_listener.IsListening)
			{
				_listener.Start();

				_processor = new Thread(() =>
				{
					var context = _listener.GetContext();
					_handler(context);
					context.Response.Close();
				}) { Name = "WebServer" };
				_processor.Start();
			}
		}

		public void Dispose()
		{
			_processor.Abort();
			_listener.Stop();
			_listener.Close();
		}
	}
}