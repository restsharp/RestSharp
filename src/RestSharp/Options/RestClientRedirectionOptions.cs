using RestSharp.Extensions;
using System.Net;
using System.Reflection;

namespace RestSharp;

/// <summary>
/// Options related to redirect processing.
/// </summary>
[GenerateImmutable]
public class RestClientRedirectionOptions {
    static readonly Version Version = new AssemblyName(typeof(RestClientOptions).Assembly.FullName!).Version!;

    /// <summary>
    /// Set to true (default), when you want to follow redirects
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// Set to true (default is false), when you want to follow a
    /// redirect from HTTPS to HTTP.
    /// </summary>
    public bool FollowRedirectsToInsecure { get; set; } = false;

    /// <summary>
    /// Set to true (default), when you want to include the originally
    /// requested headers in redirected requests.
    /// </summary>
    public bool ForwardHeaders { get; set; } = true;

    /// <summary>
    /// Set to true (default is false), when you want to send the original
    /// Authorization header to the redirected destination.
    /// </summary>
    public bool ForwardAuthorization { get; set; } = false;

    /// <summary>
    /// Set to true (default), when you want to include cookies from the 
    /// <see cref="CookieContainer"/> on the redirected URL.
    /// </summary>
    /// <remarks>
    /// NOTE: The exact cookies sent to the redirected url DEPENDS directly
    /// on the redirected url. A redirection to a completly differnet FQDN
    /// for example is unlikely to actually propagate any cookies from the 
    /// <see cref="CookieContainer"/>.
    /// </remarks>
    public bool ForwardCookies { get; set; } = true;

    /// <summary>
    /// Set to true (default) in order to send the body to the
    /// redirected URL, unless the force verb to GET behavior is triggered.
    /// <see cref="ForceForwardBody"/>
    /// </summary>
    public bool ForwardBody { get; set; } = true;

    /// <summary>
    /// Set to true (default is false) to force forwarding the body of the 
    /// request even when normally, the verb might be altered to GET based
    /// on backward compatiblity with browser processing of HTTP status codes.
    /// </summary>
    /// <remarks>
    /// Based on Wikipedia https://en.wikipedia.org/wiki/HTTP_302:
    /// <pre>
    ///  Many web browsers implemented this code in a manner that violated this standard, changing
    ///  the request type of the new request to GET, regardless of the type employed in the original request
    ///  (e.g. POST). For this reason, HTTP/1.1 (RFC 2616) added the new status codes 303 and 307 to disambiguate
    ///  between the two behaviours, with 303 mandating the change of request type to GET, and 307 preserving the
    ///  request type as originally sent. Despite the greater clarity provided by this disambiguation, the 302 code
    ///  is still employed in web frameworks to preserve compatibility with browsers that do not implement the HTTP/1.1
    ///  specification.
    /// </pre>
    /// </remarks>
    public bool ForceForwardBody { get; set; } = false;

    /// <summary>
    /// Set to true (default) to forward the query string to the redirected URL.
    /// </summary>
    public bool ForwardQuery { get; set; } = true;

    /// <summary>
    /// The maximum number of redirects to follow.
    /// </summary>
    public int MaxRedirects { get; set; } = 10;

    /// <summary>
    /// Set to true (default), to supply any requested fragment portion of the original URL to the destination URL.
    /// </summary>
    /// <remarks>
    /// Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a
    /// fragment should inherit the fragment from the original URI.
    /// </remarks>
    public bool ForwardFragment { get; set; } = true;

    /// <summary>
    /// Set to true (default), to allow the HTTP Method used on the original request to
    /// be replaced with GET when the status code 303 (HttpStatusCode.RedirectMethod)
    /// was returned. Setting this to false will disallow the altering of the verb.
    /// </summary>
    public bool AllowRedirectMethodStatusCodeToAlterVerb { get; set; } = true;
 
    /// <summary>
    /// HttpStatusCodes that trigger redirect processing. Defaults to MovedPermanently (301),
    /// SeeOther/RedirectMethod (303),
    /// TemporaryRedirect (307),
    /// Redirect (302),
    /// PermanentRedirect (308)
    /// </summary>
    public IReadOnlyList<HttpStatusCode> RedirectStatusCodes { get; set; }

    public RestClientRedirectionOptions() {
        RedirectStatusCodes = new List<HttpStatusCode>() {
            HttpStatusCode.MovedPermanently,
            HttpStatusCode.SeeOther,
            HttpStatusCode.TemporaryRedirect,
            HttpStatusCode.Redirect,
#if NET
            HttpStatusCode.PermanentRedirect,
#endif
        }.AsReadOnly();
    }
}

