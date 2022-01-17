using System.Net;

namespace RestSharp.Tests.Shared.Fixtures;

public class WebServer {
    readonly HttpListener       _listener = new();
    Action<HttpListenerContext> _responderMethod;

    public WebServer(string prefix, Action<HttpListenerContext> method, AuthenticationSchemes authenticationSchemes) {
        if (string.IsNullOrEmpty(prefix))
            throw new ArgumentException("URI prefix is required");

        _listener.Prefixes.Add(prefix);
        _listener.AuthenticationSchemes = authenticationSchemes;

        _responderMethod = method;
    }

    public async Task Run(CancellationToken token) {
        var taskFactory = new TaskFactory(token);
        _listener.Start();

        try {
            while (!token.IsCancellationRequested && _listener.IsListening) {
                try {
                    var ctx = await GetContextAsync();
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (ctx == null) continue;

                    _responderMethod?.Invoke(ctx);
                    ctx.Response.OutputStream.Close();
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

        Task<HttpListenerContext> GetContextAsync()
            => taskFactory.FromAsync(
                (callback, state) => ((HttpListener)state!).BeginGetContext(callback, state),
                iar => {
                    try {
                        return ((HttpListener)iar.AsyncState!).EndGetContext(iar);
                    }
                    catch (ObjectDisposedException) {
                        // it's ok
                        return null;
                    }
                    catch (HttpListenerException) {
                        // it's ok
                        return null;
                    }
                },
                _listener
            );
    }

    public void Stop() {
        _listener.Stop();
        _listener.Close();
    }

    public void ChangeHandler(Action<HttpListenerContext> handler) => _responderMethod = handler;
}