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

namespace RestSharp;

static class ResponseHandling {
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

    public static Task<Stream?> ReadResponse(this HttpResponseMessage response, CancellationToken cancellationToken) {
#if NETSTANDARD || NETFRAMEWORK
        return response.Content.ReadAsStreamAsync();
# else
        return response.Content.ReadAsStreamAsync(cancellationToken)!;
#endif
    }
}