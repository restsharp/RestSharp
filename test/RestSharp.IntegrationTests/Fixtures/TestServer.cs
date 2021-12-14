using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RestSharp.IntegrationTests.Fixtures;

public class HttpServer {
    readonly WebApplication _app;

    const string Address = "http://localhost:5151";

    public HttpServer(ITestOutputHelper? output = null) {
        var builder = WebApplication.CreateBuilder();

        if (output != null)
            builder.WebHost.ConfigureLogging(x => x.SetMinimumLevel(LogLevel.Information).AddXunit(output, LogLevel.Debug));
        
        builder.WebHost.UseUrls(Address);
        _app = builder.Build();
        _app.MapGet("success", () => new TestResponse { Message = "Works!" });
        _app.MapGet("echo", (string msg) => msg);
        _app.MapGet("timeout", async () => await Task.Delay(2000));
        // ReSharper disable once ConvertClosureToMethodGroup
        _app.MapGet("status", (int code) => Results.StatusCode(code));
    }

    public Uri Url => new(Address);

    public Task Start() => _app.StartAsync();

    public Task Stop() => _app.StopAsync();
}