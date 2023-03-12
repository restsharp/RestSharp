using Microsoft.AspNetCore.Http;

namespace RestSharp.Tests.Integrated.Server.Handlers;

public static class FormRequestHandler {
    public static IResult HandleForm(HttpContext ctx) {
        var response = ctx.Request.Form.Select(
            x => new TestServerResponse(x.Key, x.Value)
        );
        return Results.Ok(response);
    }
}
