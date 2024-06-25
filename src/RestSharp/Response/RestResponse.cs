//   Copyright (c) .NET Foundation and Contributors
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
using System.Text;
using RestSharp.Extensions;

// ReSharper disable SuggestBaseTypeForParameter

namespace RestSharp;

/// <summary>
/// Container for data sent back from API including deserialized data
/// </summary>
/// <typeparam name="T">Type of data to deserialize to</typeparam>
[GenerateClone<RestResponse>(Name = "FromResponse")]
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}()}}")]
public partial class RestResponse<T>(RestRequest request) : RestResponse(request) {
    /// <summary>
    /// Deserialized entity data
    /// </summary>
    public T? Data { get; set; }
}

/// <summary>
/// Container for data sent back from API
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}()}}")]
public class RestResponse(RestRequest request) : RestResponseBase(request) {
    internal static async Task<RestResponse> FromHttpResponse(
        HttpResponseMessage     httpResponse,
        RestRequest             request,
        Encoding                encoding,
        CookieCollection?       cookieCollection,
        CalculateResponseStatus calculateResponseStatus,
        CancellationToken       cancellationToken
    ) {
        return request.AdvancedResponseWriter?.Invoke(httpResponse, request) ?? await GetDefaultResponse().ConfigureAwait(false);

        async Task<RestResponse> GetDefaultResponse() {
#if NET
            await using var stream = await httpResponse.ReadResponseStream(request.ResponseWriter, cancellationToken).ConfigureAwait(false);
#else
            using var stream = await httpResponse.ReadResponseStream(request.ResponseWriter, cancellationToken).ConfigureAwait(false);
#endif

            var bytes   = stream == null ? null : await stream.ReadAsBytes(cancellationToken).ConfigureAwait(false);
            var content = bytes  == null ? null : await httpResponse.GetResponseString(bytes, encoding);

            return new(request) {
                Content             = content,
                ContentEncoding     = httpResponse.Content?.Headers.ContentEncoding ?? Array.Empty<string>(),
                ContentHeaders      = httpResponse.Content?.Headers.GetHeaderParameters(),
                ContentLength       = httpResponse.Content?.Headers.ContentLength,
                ContentType         = httpResponse.Content?.Headers.ContentType?.MediaType,
                Cookies             = cookieCollection,
                ErrorException      = httpResponse.MaybeException(),
                Headers             = httpResponse.Headers.GetHeaderParameters(),
                IsSuccessStatusCode = httpResponse.IsSuccessStatusCode,
                RawBytes            = bytes,
                ResponseStatus      = calculateResponseStatus(httpResponse),
                ResponseUri         = httpResponse.RequestMessage?.RequestUri,
                RootElement         = request.RootElement,
                Server              = httpResponse.Headers.Server.ToString(),
                StatusCode          = httpResponse.StatusCode,
                StatusDescription   = httpResponse.ReasonPhrase,
                Version             = httpResponse.RequestMessage?.Version
            };
        }
    }

    public RestResponse() : this(new()) { }
}

public delegate ResponseStatus CalculateResponseStatus(HttpResponseMessage httpResponse);
