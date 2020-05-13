//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Extensions
{
    public static class StringEncodingExtensions
    {
        /// <summary>
        ///     Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        ///     http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <param name="encoding">Content encoding. Will fallback to UTF8 if not a valid encoding.</param>
        /// <returns>The byte as a string.</returns>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static string AsString(this byte[] buffer, string? encoding)
        {
            var enc = encoding.IsEmpty() ? Encoding.UTF8 : TryParseEncoding();

            return AsString(buffer, enc);

            Encoding TryParseEncoding()
            {
                try
                {
                    return Encoding.GetEncoding(encoding) ?? Encoding.UTF8;
                }
                catch (ArgumentException)
                {
                    return Encoding.UTF8;
                }
            }
        }

        /// <summary>
        ///     Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        ///     http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <returns>The byte as a string using UTF8.</returns>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static string AsString(this byte[] buffer) => AsString(buffer, Encoding.UTF8);

        static string AsString(byte[] buffer, Encoding encoding) => buffer == null ? "" : encoding.GetString(buffer, 0, buffer.Length);
    }
}
