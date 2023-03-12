using Microsoft.AspNetCore.Http;

namespace RestSharp.Tests.Integrated.Server.Handlers;

public static class CookieHandlers {
    public static IResult HandleCookies(HttpContext ctx) {
        var results = new List<string>();

        foreach (var (key, value) in ctx.Request.Cookies) {
            results.Add($"{key}={value}");
        }

        return Results.Ok(results);
    }

    public static IResult HandleSetCookies(HttpContext ctx) {
        ctx.Response.Cookies.Append("cookie1", "value1");

        ctx.Response.Cookies.Append(
            "cookie2",
            "value2",
            new CookieOptions {
                Path = "/path_extra"
            }
        );

        ctx.Response.Cookies.Append(
            "cookie3",
            "value3",
            new CookieOptions {
                Expires = DateTimeOffset.Now.AddDays(2)
            }
        );

        ctx.Response.Cookies.Append(
            "cookie4",
            "value4",
            new CookieOptions {
                MaxAge = TimeSpan.FromSeconds(100)
            }
        );

        ctx.Response.Cookies.Append(
            "cookie5",
            "value5",
            new CookieOptions {
                Secure = true
            }
        );

        ctx.Response.Cookies.Append(
            "cookie6",
            "value6",
            new CookieOptions {
                HttpOnly = true
            }
        );
        
        ctx.Response.Cookies.Append(
            "cookie_empty_domain",
            "value_empty_domain",
            new CookieOptions {
                HttpOnly = true,
                Domain = string.Empty
            }
        );
        
        return Results.Content("success");
    }
}
