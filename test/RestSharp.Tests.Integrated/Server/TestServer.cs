using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp.Tests.Integrated.Server.Handlers;
using RestSharp.Tests.Shared.Extensions;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

// ReSharper disable ConvertClosureToMethodGroup

namespace RestSharp.Tests.Integrated.Server;

public sealed class HttpServer {
    readonly WebApplication _app;

    const string Address = "http://localhost:5151";
    const string SecureAddress = "https://localhost:5152";

    public const string ContentResource = "content";
    public const string TimeoutResource = "timeout";

    public HttpServer(ITestOutputHelper? output = null) {
        var builder = WebApplication.CreateBuilder();

        if (output != null) builder.Logging.AddXunit(output, LogLevel.Debug);

        var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        builder.Services.AddControllers().AddApplicationPart(typeof(UploadController).Assembly);
        builder.WebHost.UseUrls(Address, SecureAddress).UseKestrel(options => {
            options.ListenAnyIP(5151, listenOptions => { return; });
            // Yes, this is lame, but dotnet dev-certs was giving me grief trying to export
            // the public key using an empty password... :(
            var secureString = new SecureString();
            secureString.AppendChar('b');
            secureString.AppendChar('l');
            secureString.AppendChar('a');
            secureString.AppendChar('h');
            secureString.MakeReadOnly();
            options.ListenAnyIP(5152,
                listenOptions => listenOptions.UseHttps(
                    new X509Certificate2(Path.Join(currentAssemblyPath, "Server\\testCert.pfx"),
                    secureString)));
        });
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
        _app.MapPost("redirect-forcechangeverb",
            (HttpContext ctx) => {
                string redirectDestination = "/dump-headers";
                var queryString = HttpUtility.ParseQueryString(ctx.Request.QueryString.Value ?? string.Empty);
                var urlParameter = queryString.Get("url");
                if (!string.IsNullOrEmpty(urlParameter)) {
                    redirectDestination = urlParameter;
                }
                // This forces the verb to change on the redirect to GET, unless the client has set the correct
                // RedirectOption setting. (302)
                return new RedirectWithStatusCodeResult((int)HttpStatusCode.Redirect, redirectDestination);
            });
        _app.MapPost("redirect-changeverb",
            (HttpContext ctx) => {
                string redirectDestination = "/dump-headers";
                var queryString = HttpUtility.ParseQueryString(ctx.Request.QueryString.Value ?? string.Empty);
                var urlParameter = queryString.Get("url");
                if (!string.IsNullOrEmpty(urlParameter)) {
                    redirectDestination = urlParameter;
                }
                // This allows the method to change to GET on redirect on purpose, unless the client has set the correct
                // RedirectOption setting. (303)
                return new RedirectWithStatusCodeResult((int)HttpStatusCode.RedirectMethod, redirectDestination);
            });
        _app.MapPost("redirect-keepverb",
            (HttpContext ctx) => {
                string redirectDestination = "/dump-headers";
                var queryString = HttpUtility.ParseQueryString(ctx.Request.QueryString.Value ?? string.Empty);
                var urlParameter = queryString.Get("url");
                if (!string.IsNullOrEmpty(urlParameter)) {
                    redirectDestination = urlParameter;
                }
                // This prevents the method to change on redirect. (307)
                return new RedirectWithStatusCodeResult((int)HttpStatusCode.RedirectKeepVerb, redirectDestination);
            });
        _app.MapGet("redirect-insecure", (HttpContext ctx) => {
            string destination = $"{Address}/dump-headers";
            return Results.Redirect(destination, false, true);
        });
        _app.MapGet("redirect-secure", (HttpContext ctx) => {
            string destination = $"{SecureAddress}/dump-headers";
            return Results.Redirect(destination, false, true);
        });
        _app.MapGet("redirect-countdown",
            (HttpContext ctx) => {
                string redirectDestination = "/redirect-countdown";
                var queryString = HttpUtility.ParseQueryString(ctx.Request.QueryString.Value ?? string.Empty);
                int redirectsLeft = -1;
                redirectsLeft = int.Parse(queryString.Get("n"));
                if (redirectsLeft != -1
                    && redirectsLeft > 1) {
                    redirectDestination = $"{redirectDestination}?n={redirectsLeft - 1}";
                    return Results.Redirect(redirectDestination, false, true);
                }
                return Results.Ok("Stopped redirection countdown!");
            });

        _app.MapGet("dump-headers",
            (HttpContext ctx) => {
                var headers = ctx.Request.Headers;
                StringBuilder sb = new StringBuilder();
                foreach (var kvp in headers) {
                    sb.Append($"'{kvp.Key}': '{kvp.Value}',");
                }
                return new TestResponse { Message = sb.ToString() };
            });

        _app.MapGet("dump-request", DumpRequest);
        _app.MapPut("dump-request", DumpRequest);
        _app.MapPost("dump-request", DumpRequest);
        _app.MapDelete("dump-request", DumpRequest);

        // Cookies
        _app.MapGet("get-cookies", CookieHandlers.HandleCookies);
        _app.MapPut("get-cookies",
            (HttpContext cxt) => {
                // Make sure we get the status code we expect:
                return Results.StatusCode(405);
            });
        _app.MapGet("set-cookies", CookieHandlers.HandleSetCookies);
        _app.MapGet("redirect", () => Results.Redirect("/success", false, true));
        _app.MapGet(
            "get-cookies-redirect",
            (HttpContext ctx) => {
                ctx.Response.Cookies.Append("redirectCookie", "value1");
                string redirectDestination = "/get-cookies";
                var queryString = HttpUtility.ParseQueryString(ctx.Request.QueryString.Value ?? string.Empty);
                var urlParameter = queryString.Get("url");
                if (!string.IsNullOrEmpty(urlParameter)) {
                    redirectDestination = urlParameter;
                }
                return Results.Redirect(redirectDestination, false, true);
            }
        );

        _app.MapPost(
            "/post/set-cookie-redirect",
            (HttpContext ctx) => {
                ctx.Response.Cookies.Append("redirectCookie", "value1");
                return Results.Redirect("/get-cookies", permanent: false, preserveMethod: false);
            });
        _app.MapPost(
            "/post/set-cookie-seeother",
            (HttpContext ctx) => {
                ctx.Response.Cookies.Append("redirectCookie", "seeOtherValue1");
                return new RedirectWithStatusCodeResult((int)HttpStatusCode.SeeOther, "/get-cookies");
            });
        _app.MapPut(
            "/put/set-cookie-redirect",
            (HttpContext ctx) => {
                ctx.Response.Cookies.Append("redirectCookie", "putCookieValue1");
                return Results.Redirect("/get-cookies", permanent: false, preserveMethod: false);
            });

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

        // Dump the request verb and body into the response.
        TestResponse DumpRequest(HttpContext ctx) {
            var method = ctx.Request.Method;
            var task = ctx.Request.Body.StreamToStringAsync();
            task.Wait();
            var body = task.Result;
            return new TestResponse { Message = $"Method: {method}\r\nBody: {body}" };
        }
    }

    public Uri Url => new(Address);
    public Uri SecureUrl => new(SecureAddress);

    public Task Start() => _app.StartAsync();

    public async Task Stop() {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
