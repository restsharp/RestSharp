//   Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using RestSharp.Extensions;

// ReSharper disable SuggestBaseTypeForParameter

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
            Content           = response.Content,
            RawBytes          = response.RawBytes,
            ContentEncoding   = response.ContentEncoding,
            ContentLength     = response.ContentLength,
            ContentType       = response.ContentType,
            Cookies           = response.Cookies,
            ErrorMessage      = response.ErrorMessage,
            ErrorException    = response.ErrorException,
            Headers           = response.Headers,
            ContentHeaders    = response.ContentHeaders,
            IsSuccessful      = response.IsSuccessful,
            ResponseStatus    = response.ResponseStatus,
            ResponseUri       = response.ResponseUri,
            Server            = response.Server,
            StatusCode        = response.StatusCode,
            StatusDescription = response.StatusDescription,
            Request           = response.Request,
            ResponseMessage   = response.ResponseMessage
        };
}

/// <summary>
/// Container for data sent back from API
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
public class RestResponse : RestResponseBase {
    internal static async Task<RestResponse> FromHttpResponse(
        HttpResponseMessage httpResponse,
        RestRequest         request,
        CookieCollection    cookieCollection,
        CancellationToken   cancellationToken
    ) {
        return request.AdvancedResponseWriter?.Invoke(httpResponse) ?? await GetDefaultResponse();

        async Task<RestResponse> GetDefaultResponse() {
            byte[]? bytes;
            string? content;

            if (request.ResponseWriter != null) {
#if NETSTANDARD
                var stream = await httpResponse.Content.ReadAsStreamAsync();
# else
                var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
#endif
                var converted = request.ResponseWriter(stream);

                if (converted == null) {
                    bytes   = null;
                    content = null;
                }
                else {
                    bytes = await converted.ReadAsBytes(cancellationToken);
                    var encodingString = httpResponse.Content.Headers.ContentEncoding.FirstOrDefault();
                    var encoding       = encodingString != null ? Encoding.GetEncoding(encodingString) : Encoding.UTF8;
                    content = encoding.GetString(bytes);
                }
            }
            else {
#if NETSTANDARD
                bytes   = await httpResponse.Content.ReadAsByteArrayAsync();
                content = await httpResponse.Content.ReadAsStringAsync();
# else
                bytes = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
#endif
            }

            return new RestResponse {
                Content           = content,
                RawBytes          = bytes,
                ContentEncoding   = httpResponse.Content.Headers.ContentEncoding,
                Version           = httpResponse.RequestMessage?.Version,
                ContentLength     = httpResponse.Content.Headers.ContentLength,
                ContentType       = httpResponse.Content.Headers.ContentType?.MediaType,
                ResponseStatus    = httpResponse.IsSuccessStatusCode ? ResponseStatus.Completed : ResponseStatus.Error,
                ResponseUri       = httpResponse.RequestMessage!.RequestUri,
                Server            = httpResponse.Headers.Server.ToString(),
                StatusCode        = httpResponse.StatusCode,
                StatusDescription = httpResponse.ReasonPhrase,
                IsSuccessful      = httpResponse.IsSuccessStatusCode,
                Request           = request,
                ResponseMessage   = httpResponse,
                Headers           = httpResponse.Headers.GetHeaderParameters(),
                ContentHeaders    = httpResponse.Content.Headers.GetHeaderParameters(),
                Cookies           = cookieCollection
            };
        }
    }
}