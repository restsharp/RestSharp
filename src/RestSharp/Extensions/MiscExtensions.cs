#region License

//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

#endregion

using System;
using System.IO;
using System.Text;

namespace RestSharp.Extensions
{
    /// <summary>
    ///     Extension method overload!
    /// </summary>
    public static class MiscExtensions
    {
        /// <summary>
        ///     Save a byte array to a file
        /// </summary>
        /// <param name="input">Bytes to save</param>
        /// <param name="path">Full path to save file to</param>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static void SaveAs(this byte[] input, string path) => File.WriteAllBytes(path, input);

        /// <summary>
        ///     Read a stream into a byte array
        /// </summary>
        /// <param name="input">Stream to read</param>
        /// <returns>byte[]</returns>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static byte[] ReadAsBytes(this Stream input)
        {
            var buffer = new byte[16 * 1024];

            using var ms = new MemoryStream();

            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer, 0, read);

            return ms.ToArray();
        }

        /// <summary>
        ///     Copies bytes from one stream to another
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static void CopyTo(this Stream input, Stream output)
        {
            var buffer = new byte[32768];

            while (true)
            {
                var read = input.Read(buffer, 0, buffer.Length);

                if (read <= 0)
                    return;

                output.Write(buffer, 0, read);
            }
        }

        /// <summary>
        ///     Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        ///     http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <param name="encoding">Content encoding. Will fallback to UTF8 if not a valid encoding.</param>
        /// <returns>The byte as a string.</returns>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static string AsString(this byte[] buffer, string encoding)
        {
            var enc = Encoding.UTF8;

            try
            {
                if (!string.IsNullOrEmpty(encoding))
                    enc = Encoding.GetEncoding(encoding);
            }
            catch (Exception)
            {
                // Use UTF8 as the default
            }

            return AsString(buffer, enc);
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