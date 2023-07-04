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
//

using System.Text;

namespace RestSharp.Extensions;

static class HttpResponseExtensions {
    public static Exception? MaybeException(this HttpResponseMessage httpResponse)
        => httpResponse.IsSuccessStatusCode
            ? null
#if NET
            : new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}", null, httpResponse.StatusCode);
#else
            : new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}");
#endif

    public static string GetResponseString(this HttpResponseMessage response, byte[] bytes, Encoding clientEncoding) {
        var encodingString = response.Content.Headers.ContentType?.CharSet;
        var encoding       = encodingString != null ? TryGetEncoding(encodingString) : clientEncoding;

        using var reader = new StreamReader(new MemoryStream(bytes), encoding);
        return reader.ReadToEnd();

        Encoding TryGetEncoding(string es) {
            try {
                return Encoding.GetEncoding(es);
            }
            catch {
                return Encoding.Default;
            }
        }
    }

    public static Task<Stream?> ReadResponseStream(
        this HttpResponseMessage httpResponse,
        Func<Stream, Stream?>?   writer,
        CancellationToken        cancellationToken = default
    ) {
        var readTask = writer == null ? ReadResponse() : ReadAndConvertResponse(writer);
        return readTask;

        Task<Stream?> ReadResponse() {
#if NET
            return httpResponse.Content.ReadAsStreamAsync(cancellationToken)!;
# else
            return httpResponse.Content == null ? Task.FromResult((Stream?)null) : httpResponse.Content.ReadAsStreamAsync();
#endif
        }

        async Task<Stream?> ReadAndConvertResponse(Func<Stream, Stream?> streamWriter) {
#if NET
            await using var original = await ReadResponse().ConfigureAwait(false);
#else
            using var original = await ReadResponse().ConfigureAwait(false);
#endif
            return original == null ? null : streamWriter(original);
        }
    }
}
