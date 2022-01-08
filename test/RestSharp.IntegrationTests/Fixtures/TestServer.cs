using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.IntegrationTests.Fixtures;

public class TestServerFixture : IAsyncLifetime {
    public HttpServer Server { get; } = new();

    public Task InitializeAsync() => Server.Start();

    public Task DisposeAsync() => Server.Stop();
}

[CollectionDefinition(nameof(TestServerCollection))]
public class TestServerCollection : ICollectionFixture<TestServerFixture> { }

public sealed class HttpServer {
    readonly WebApplication _app;

    const string Address = "http://localhost:5151";

    public HttpServer(ITestOutputHelper output = null) {
        var builder = WebApplication.CreateBuilder();

        if (output != null)
            builder.WebHost.ConfigureLogging(x => x.SetMinimumLevel(LogLevel.Information).AddXunit(output, LogLevel.Debug));

        builder.WebHost.UseUrls(Address);
        _app = builder.Build();

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // GET
        _app.MapGet("success", () => new TestResponse { Message = "Works!" });
        _app.MapGet("echo", (string msg) => msg);
        _app.MapGet("timeout", async () => await Task.Delay(2000));
        _app.MapPut("timeout", async () => await Task.Delay(2000));
        // ReSharper disable once ConvertClosureToMethodGroup
        _app.MapGet("status", (int code) => Results.StatusCode(code));
        _app.MapGet("headers", HandleHeaders);

        // PUT
        _app.MapPut(
            "content",
            async context => {
                var content  = await context.Request.Body.StreamToStringAsync();
                await context.Response.WriteAsync(content);
            }
        );

        IResult HandleHeaders(HttpContext ctx) {
            var response = ctx.Request.Headers.Select(x => new TestServerResponse(x.Key, x.Value));
            return Results.Ok(response);
        }
    }

    public Uri Url => new(Address);

    public Task Start() => _app.StartAsync();

    public async Task Stop() {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}

record TestServerResponse(string Name, string Value);

record ContentResponse(string Content);