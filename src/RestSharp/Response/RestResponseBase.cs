//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics;
using System.Net;

namespace RestSharp;

/// <summary>
/// Base class for common properties shared by RestResponse and RestResponse[[T]]
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
public abstract class RestResponseBase {
    /// <summary>
    /// Default constructor
    /// </summary>
    protected RestResponseBase() => ResponseStatus = ResponseStatus.None;

    /// <summary>
    /// The RestRequest that was made to get this RestResponse
    /// </summary>
    /// <remarks>
    /// Mainly for debugging if ResponseStatus is not OK
    /// </remarks>
    public RestRequest? Request { get; set; }

    /// <summary>
    /// MIME content type of response
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Length in bytes of the response content
    /// </summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// Encoding of the response content
    /// </summary>
    public ICollection<string> ContentEncoding { get; set; } = new List<string>();

    /// <summary>
    /// String representation of response content
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// HTTP response status code
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Whether or not the HTTP response status code indicates success
    /// </summary>
    public bool IsSuccessStatusCode { get; set; }

    /// <summary>
    /// Whether or not the HTTP response status code indicates success and no other error occurred (deserialization, timeout, ...)
    /// </summary>
    public bool IsSuccessful => IsSuccessStatusCode && ResponseStatus == ResponseStatus.Completed;

    /// <summary>
    /// Description of HTTP status returned
    /// </summary>
    public string? StatusDescription { get; set; }

    /// <summary>
    /// Response content
    /// </summary>
    public byte[]? RawBytes { get; set; }

    /// <summary>
    /// The URL that actually responded to the content (different from request if redirected)
    /// </summary>
    public Uri? ResponseUri { get; set; }

    /// <summary>
    /// HttpWebResponse.Server
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Cookies returned by server with the response
    /// </summary>
    public CookieCollection? Cookies { get; set; }

    /// <summary>
    /// Response headers returned by server with the response
    /// </summary>
    public IReadOnlyCollection<HeaderParameter>? Headers { get; set; }

    /// <summary>
    /// Content headers returned by server with the response
    /// </summary>
    public IReadOnlyCollection<HeaderParameter>? ContentHeaders { get; set; }

    /// <summary>
    /// Status of the request. Will return Error for transport errors.
    /// HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
    /// </summary>
    public ResponseStatus ResponseStatus { get; set; }

    /// <summary>
    /// Transport or other non-HTTP error generated while attempting request
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The exception thrown during the request, if any
    /// </summary>
    public Exception? ErrorException { get; set; }

    /// <summary>
    /// HTTP protocol version of the request
    /// </summary>
    public Version? Version { get; set; }
    
    /// <summary>
    /// Root element of the serialized response content, only works if deserializer supports it 
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// Assists with debugging responses by displaying in the debugger output
    /// </summary>
    /// <returns></returns>
    protected string DebuggerDisplay() => $"StatusCode: {StatusCode}, Content-Type: {ContentType}, Content-Length: {ContentLength})";

    internal Exception? GetException()
        => ResponseStatus switch {
            ResponseStatus.Aborted   => new HttpRequestException("Request aborted", ErrorException),
            ResponseStatus.Error     => ErrorException,
            ResponseStatus.TimedOut  => new TimeoutException("Request timed out", ErrorException),
            ResponseStatus.None      => null,
            ResponseStatus.Completed => null,
            _                        => throw ErrorException ?? new ArgumentOutOfRangeException(nameof(ResponseStatus))
        };
}