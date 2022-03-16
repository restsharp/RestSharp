using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp.Extensions;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Integrated.Server;

public sealed class HttpServer {
    readonly WebApplication _app;

    const string Address = "http://localhost:5151";

    public const string ContentResource = "content";
    public const string TimeoutResource = "timeout";

    public HttpServer(ITestOutputHelper output = null) {
        var builder = WebApplication.CreateBuilder();

        if (output != null)
            builder.WebHost.ConfigureLogging(x => x.SetMinimumLevel(LogLevel.Information).AddXunit(output, LogLevel.Debug));
        
        builder.WebHost.UseUrls(Address);
        _app = builder.Build();

        // GET
        _app.MapGet("success", () => new TestResponse { Message = "Works!" });
        _app.MapGet("echo", (string msg) => msg);
        _app.MapGet(TimeoutResource, async () => await Task.Delay(2000));
        _app.MapPut(TimeoutResource, async () => await Task.Delay(2000));
        // ReSharper disable once ConvertClosureToMethodGroup
        _app.MapGet("status", (int code) => Results.StatusCode(code));
        _app.MapGet("headers", HandleHeaders);
        _app.MapGet("request-echo", async context => await context.Request.BodyReader.AsStream().CopyToAsync(context.Response.BodyWriter.AsStream()));
        _app.MapDelete("delete", () => new TestResponse { Message = "Works!" });

        // Cookies
        _app.MapGet("get-cookies", HandleCookies);
        _app.MapGet("set-cookies", HandleSetCookies);

        // PUT
        _app.MapPut(
            ContentResource,
            async context => {
                var content = await context.Request.Body.StreamToStringAsync();
                await context.Response.WriteAsync(content);
            }
        );

        // Upload file
        var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

        _app.MapPost("/upload", HandleUpload);
        
        // POST
        _app.MapPost("/post/json", (TestRequest request) => new TestResponse { Message = request.Data });

        IResult HandleHeaders(HttpContext ctx) {
            var response = ctx.Request.Headers.Select(x => new TestServerResponse(x.Key, x.Value));
            return Results.Ok(response);
        }

        IResult HandleCookies(HttpContext ctx) {
            var results = new List<string>();
            foreach (var (key, value) in ctx.Request.Cookies) {
                results.Add($"{key}={value}");
            }
            return Results.Ok(results);
        }

        IResult HandleSetCookies(HttpContext ctx) {
            ctx.Response.Cookies.Append("cookie1", "value1");
            ctx.Response.Cookies.Append("cookie2", "value2", new CookieOptions {
                Path = "/path_extra"
            });
            ctx.Response.Cookies.Append("cookie3", "value3", new CookieOptions {
                Expires = DateTimeOffset.Now.AddDays(2)
            });
            ctx.Response.Cookies.Append("cookie4", "value4", new CookieOptions {
                MaxAge = TimeSpan.FromSeconds(100)
            });
            ctx.Response.Cookies.Append("cookie5", "value5", new CookieOptions {
                Secure = true
            });
            ctx.Response.Cookies.Append("cookie6", "value6", new CookieOptions {
                HttpOnly = true
            });
            return Results.Content("success");
        }

        async Task<IResult> HandleUpload(HttpRequest req) {
            if (!req.HasFormContentType) {
                return Results.BadRequest("It's not a form");
            }

            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file is null) {
                return Results.BadRequest("File parameter 'file' is not present");
            }

            await using var stream = file.OpenReadStream();

            var received = await stream.ReadAsBytes(default);
            var expected = await File.ReadAllBytesAsync(Path.Combine(assetPath, file.FileName));

            return Results.Ok(new UploadResponse(file.FileName, file.Length, received.SequenceEqual(expected)));
        }
    }

    public Uri Url => new(Address);

    public Task Start() => _app.StartAsync();

    public async Task Stop() {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}