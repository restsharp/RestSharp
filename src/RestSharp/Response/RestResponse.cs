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
            Request           = response.Request
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
        Encoding            encoding,
        CookieCollection    cookieCollection,
        CancellationToken   cancellationToken
    ) {
        return request.AdvancedResponseWriter?.Invoke(httpResponse) ?? await GetDefaultResponse().ConfigureAwait(false);

        async Task<RestResponse> GetDefaultResponse() {
            var       readTask = request.ResponseWriter == null ? ReadResponse() : ReadAndConvertResponse();
            using var stream   = await readTask.ConfigureAwait(false);

            var bytes   = stream == null ? null : await stream.ReadAsBytes(cancellationToken).ConfigureAwait(false);
            var content = bytes == null ? null : httpResponse.GetResponseString(bytes, encoding);

            return new RestResponse {
                Content           = content,
                RawBytes          = bytes,
                ContentEncoding   = httpResponse.Content.Headers.ContentEncoding,
                Version           = httpResponse.RequestMessage?.Version,
                ContentLength     = httpResponse.Content.Headers.ContentLength,
                ContentType       = httpResponse.Content.Headers.ContentType?.MediaType,
                ResponseStatus    = httpResponse.IsSuccessStatusCode ? ResponseStatus.Completed : ResponseStatus.Error,
                ErrorException    = MaybeException(),
                ResponseUri       = httpResponse.RequestMessage!.RequestUri,
                Server            = httpResponse.Headers.Server.ToString(),
                StatusCode        = httpResponse.StatusCode,
                StatusDescription = httpResponse.ReasonPhrase,
                IsSuccessful      = httpResponse.IsSuccessStatusCode,
                Request           = request,
                Headers           = httpResponse.Headers.GetHeaderParameters(),
                ContentHeaders    = httpResponse.Content.Headers.GetHeaderParameters(),
                Cookies           = cookieCollection
            };

            Exception? MaybeException()
                => httpResponse.IsSuccessStatusCode
                    ? null
                    : new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}");

            Task<Stream?> ReadResponse() => httpResponse.ReadResponse(cancellationToken);

            async Task<Stream?> ReadAndConvertResponse() {
                using var original = await ReadResponse().ConfigureAwait(false);
                return request.ResponseWriter!(original!);
            }
        }
    }
}