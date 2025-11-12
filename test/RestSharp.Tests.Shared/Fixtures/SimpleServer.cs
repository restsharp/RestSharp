using System.Net;

namespace RestSharp.Tests.Shared.Fixtures;

public sealed class SimpleServer : IDisposable {
    static readonly Random Random = new(DateTimeOffset.Now.Millisecond);

    readonly WebServer               _server;
    readonly CancellationTokenSource _cts = new();

    public string Url { get; }

    SimpleServer(
        int                         port,
        Action<HttpListenerContext> handler               = null,
        AuthenticationSchemes       authenticationSchemes = AuthenticationSchemes.Anonymous
    ) {
        Url     = $"http://localhost:{port}/";
        _server = new(Url, handler, authenticationSchemes);
        Task.Run(() => _server.Run(_cts.Token));
    }

    public void Dispose() {
        _cts.Cancel();
        _server.Stop();
        _cts.Dispose();
    }

    public static SimpleServer Create(
        Action<HttpListenerContext> handler               = null,
        AuthenticationSchemes       authenticationSchemes = AuthenticationSchemes.Anonymous
    ) {
        var port = Random.Next(1000, 9999);
        return new(port, handler, authenticationSchemes);
    }

    public void SetHandler(Action<HttpListenerContext> handler) => _server.ChangeHandler(handler);
}