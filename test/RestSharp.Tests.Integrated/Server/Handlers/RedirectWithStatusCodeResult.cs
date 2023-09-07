using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Tests.Integrated.Server.Handlers;

/// <summary>
/// An <see cref="IResult"/> that returns a redirection with a supplied status code value. 
/// Created in order to easily return a SeeOther status code.
/// </summary>
class RedirectWithStatusCodeResult : IResult {
    public int StatusCode { get; }
    public string Uri { get; }

    public RedirectWithStatusCodeResult(int statusCode, string url) {
        Uri = url;
        StatusCode = statusCode;
    }

    public Task ExecuteAsync(HttpContext httpContext) {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.Headers.Location = Uri;

        return Task.CompletedTask;
    }
}
