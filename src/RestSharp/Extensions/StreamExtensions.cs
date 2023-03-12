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

namespace RestSharp.Extensions;

/// <summary>
/// Extension method overload!
/// </summary>
static class StreamExtensions {
    /// <summary>
    /// Read a stream into a byte array
    /// </summary>
    /// <param name="input">Stream to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns>byte[]</returns>
    public static async Task<byte[]> ReadAsBytes(this Stream input, CancellationToken cancellationToken) {
        var buffer = new byte[16 * 1024];

        using var ms = new MemoryStream();

        int read;
#if NETSTANDARD || NETFRAMEWORK
        while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#else
        while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
#endif
            ms.Write(buffer, 0, read);

        return ms.ToArray();
    }
}