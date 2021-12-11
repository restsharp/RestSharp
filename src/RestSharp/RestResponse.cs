using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using RestSharp.Extensions;

namespace RestSharp;

/// <summary>
/// Container for data sent back from API including deserialized data
/// </summary>
/// <typeparam name="T">Type of data to deserialize to</typeparam>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
public class RestResponse<T> : RestResponse {
    /// <summary>
    /// Deserialized entity data
    /// </summary>
    public T? Data { get; internal set; }

    public static RestResponse<T> FromResponse(RestResponse response)
        => new() {
            ContentEncoding   = response.ContentEncoding,
            ContentLength     = response.ContentLength,
            ContentType       = response.ContentType,
            Cookies           = response.Cookies,
            ErrorMessage      = response.ErrorMessage,
            ErrorException    = response.ErrorException,
            Headers           = response.Headers,
            RawBytes          = response.RawBytes,
            ResponseStatus    = response.ResponseStatus,
            ResponseUri       = response.ResponseUri,
            Server            = response.Server,
            StatusCode        = response.StatusCode,
            StatusDescription = response.StatusDescription,
            Request           = response.Request
        };
}

/// <summary>
/// Container for data sent back from API
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
public class RestResponse : RestResponseBase {
    RestResponse SetHeaders(HttpResponseHeaders headers) {
        var headerParams = headers
            .SelectMany(x => x.Value.Select(y => (x.Key, y)))
            .Select(x => new Parameter(x.Key, x.y, ParameterType.HttpHeader))
            .ToList();
        return this.With(x => x.Headers = headerParams);
    }

    RestResponse SetCookies(CookieCollection cookies) => this.With(x => x.Cookies = cookies);

    internal static async Task<RestResponse> FromHttpResponse(
        HttpResponseMessage httpResponse,
        IRestRequest        request,
        CookieCollection    cookieCollection,
        CancellationToken cancellationToken
    )
        => new RestResponse {
                #if NETSTANDARD
                Content           = await httpResponse.Content.ReadAsStringAsync(),
                RawBytes          = await httpResponse.Content.ReadAsByteArrayAsync(),
                # else
                Content           = await httpResponse.Content.ReadAsStringAsync(cancellationToken),
                RawBytes          = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken),
                #endif
                ContentEncoding   = httpResponse.Content.Headers.ContentEncoding,
                ContentLength     = httpResponse.Content.Headers.ContentLength,
                ContentType       = httpResponse.Content.Headers.ContentType?.MediaType,
                ResponseStatus    = httpResponse.IsSuccessStatusCode ? ResponseStatus.Completed : ResponseStatus.Error,
                ResponseUri       = httpResponse.RequestMessage!.RequestUri,
                Server            = httpResponse.Headers.Server.ToString(),
                StatusCode        = httpResponse.StatusCode,
                StatusDescription = httpResponse.ReasonPhrase,
                IsSuccessful      = httpResponse.IsSuccessStatusCode,
                Request           = request
            }
            .SetHeaders(httpResponse.Headers)
            .SetCookies(cookieCollection);
}