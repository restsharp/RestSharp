using System.Net;
using System.Reflection;

namespace RestSharp.Tests.Shared.Fixtures; 

public static class Handlers {
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

            if (method!.IsStatic)
                method.Invoke(null, [ctx]);
            else
                method.Invoke(new T(), [ctx]);
        };
}