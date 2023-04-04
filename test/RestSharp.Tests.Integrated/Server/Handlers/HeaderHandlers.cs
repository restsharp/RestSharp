using Microsoft.AspNetCore.Http;

namespace RestSharp.Tests.Integrated.Server.Handlers;

public static class HeaderHandlers {
    public static IResult HandleHeaders(HttpContext ctx) {
        var response = ctx.Request.Headers.Select(x => new TestServerResponse(x.Key, x.Value!));
        return Results.Ok(response);
    }
}
