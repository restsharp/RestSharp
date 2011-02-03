using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace RestSharp.IntegrationTests.Helpers
{
	public static class Handlers
	{
		/// <summary>
		/// Echoes the request input back to the output.
		/// </summary>
		public static void Echo(HttpListenerContext context)
		{
			context.Request.InputStream.CopyTo(context.Response.OutputStream);
		}

		/// <summary>
		/// Echoes the given value back to the output.
		/// </summary>
		public static Action<HttpListenerContext> EchoValue(string value)
		{
			return ctx => ctx.Response.OutputStream.WriteStringUtf8(value);
		}

		/// <summary>
		/// Response to a request like this:  http://localhost:8080/assets/koala.jpg
		/// by streaming the file located at "assets\koala.jpg" back to the client.
		/// </summary>
		public static void FileHandler(HttpListenerContext context)
		{
			var pathToFile = Path.Combine(context.Request.Url.Segments.Select(s => s.Replace("/", "")).ToArray());

			using(var reader = new StreamReader(pathToFile))
				reader.BaseStream.CopyTo(context.Response.OutputStream);
		}

		/// <summary>
		/// T should be a class that implements methods whose names match the urls being called, and take one parameter, an HttpListenerContext.
		/// e.g.
		/// urls exercised:  "http://localhost:8080/error"  and "http://localhost:8080/get_list"
		/// 
		/// class MyHandler
		/// {
		///   void error(HttpListenerContext ctx)
		///   {
		///     // do something interesting here
		///   }
		///
		///   void get_list(HttpListenerContext ctx)
		///   {
		///     // do something interesting here
		///   }
		/// }
		/// </summary>
		public static Action<HttpListenerContext> Generic<T>() where T : new()
		{
			return ctx =>
			{
				var methodName = ctx.Request.Url.Segments.Last();
				var method = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

				if(method.IsStatic)
				{
					method.Invoke(null, new object[] { ctx });
				}
				else
				{
					method.Invoke(new T(), new object[] { ctx });
				}
			};
		}
	}
}