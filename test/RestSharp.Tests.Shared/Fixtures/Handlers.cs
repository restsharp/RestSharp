using System.Net;
using System.Reflection;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Shared.Fixtures; 

public static class Handlers {
    /// <summary>
    /// Echoes the request input back to the output.
    /// </summary>
    public static void Echo(HttpListenerContext context) => context.Request.InputStream.CopyTo(context.Response.OutputStream);

    /// <summary>
    /// Echoes the given value back to the output.
    /// </summary>
    public static Action<HttpListenerContext> EchoValue(string value) => ctx => ctx.Response.OutputStream.WriteStringUtf8(value);

    /// <summary>
    /// Response to a request like this:  http://localhost:8888/assets/koala.jpg
    /// by streaming the file located at "assets\koala.jpg" back to the client.
    /// </summary>
    public static void FileHandler(HttpListenerContext context, string path) {
        var pathToFile = Path.Combine(
            path,
            Path.Combine(
                context.Request.Url.Segments.Select(s => s.Replace("/", "")).ToArray()
            )
        );

        using var reader = new StreamReader(pathToFile);

        reader.BaseStream.CopyTo(context.Response.OutputStream);
    }

    /// <summary>
    /// T should be a class that implements methods whose names match the urls being called, and take one parameter, an
    /// HttpListenerContext.
    /// e.g.
    /// urls exercised:  "http://localhost:8888/error"  and "http://localhost:8888/get_list"
    /// class MyHandler
    /// {
    /// void error(HttpListenerContext ctx)
    /// {
    /// // do something interesting here
    /// }
    /// void get_list(HttpListenerContext ctx)
    /// {
    /// // do something interesting here
    /// }
    /// }
    /// </summary>
    public static Action<HttpListenerContext> Generic<T>() where T : new()
        => ctx => {
            var methodName = ctx.Request.Url.Segments.Last();

            var method = typeof(T).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            );

            if (method.IsStatic)
                method.Invoke(null, new object[] { ctx });
            else
                method.Invoke(new T(), new object[] { ctx });
        };
}