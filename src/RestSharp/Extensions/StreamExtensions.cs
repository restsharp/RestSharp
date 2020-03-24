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

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Extensions
{
    internal static class StreamExtensions
    {
        public static void WriteString(this Stream stream, string value, Encoding encoding)
        {
            var bytes = encoding.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);
        }
        
        public static Task WriteStringAsync(this Stream stream, string value, Encoding encoding, CancellationToken cancellationToken)
        {
            var bytes = encoding.GetBytes(value);

            return stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }
    }
}