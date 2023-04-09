using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp.Tests.Integrated.Server.Handlers;
using RestSharp.Tests.Shared.Extensions;
// ReSharper disable ConvertClosureToMethodGroup

namespace RestSharp.Tests.Integrated.Server;

public sealed class HttpServer {
    readonly WebApplication _app;

    const string Address = "http://localhost:5151";

    public const string ContentResource = "content";
    public const string TimeoutResource = "timeout";

    public HttpServer(ITestOutputHelper? output = null) {
        var builder = WebApplication.CreateBuilder();

        if (output != null) builder.Logging.AddXunit(output, LogLevel.Debug);

        builder.Services.AddControllers().AddApplicationPart(typeof(UploadController).Assembly);
        builder.WebHost.UseUrls(Address);
        _app = builder.Build();

        _app.MapControllers();

        _app.MapGet("success", () => new TestResponse { Message = "Works!" });
        _app.MapGet("echo", (string msg) => msg);
        _app.MapGet(TimeoutResource, async () => await Task.Delay(2000));
        _app.MapPut(TimeoutResource, async () => await Task.Delay(2000));
        _app.MapGet("status", (int code) => Results.StatusCode(code));
        _app.MapGet("headers", HeaderHandlers.HandleHeaders);
        _app.MapGet("request-echo", async context => await context.Request.BodyReader.AsStream().CopyToAsync(context.Response.BodyWriter.AsStream()));
        _app.MapDelete("delete", () => new TestResponse { Message = "Works!" });

        // Cookies
        _app.MapGet("get-cookies", CookieHandlers.HandleCookies);
        _app.MapGet("set-cookies", CookieHandlers.HandleSetCookies);
        _app.MapGet("redirect", () => Results.Redirect("/success", false, true));

        // PUT
        _app.MapPut(
            ContentResource,
            async context => {
                var content = await context.Request.Body.StreamToStringAsync();
                await context.Response.WriteAsync(content);
            }
        );

        // Upload file
        // var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        // _app.MapPost("/upload", ctx => FileHandlers.HandleUpload(assetPath, ctx.Request));

        // POST
        _app.MapPost("/post/json", (TestRequest request) => new TestResponse { Message = request.Data });

        _app.MapPost(
            "/post/form",
            (HttpContext context) => new TestResponse { Message = $"Works! Length: {context.Request.Form["big_string"].ToString().Length}" }
        );

        _app.MapPost("/post/data", FormRequestHandler.HandleForm);
    }

    public Uri Url => new(Address);

    public Task Start() => _app.StartAsync();

    public async Task Stop() {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
