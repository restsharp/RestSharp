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

using System;
using System.IO;

namespace RestSharp
{
    /// <summary>
    ///     Container for HTTP file
    /// </summary>
    public class HttpFile
    {
        /// <summary>
        ///     The length of data to be sent
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        ///     Provides raw data for file
        /// </summary>
        public Action<Stream> Writer { get; set; }

        /// <summary>
        ///     Name of the file to use when uploading
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     MIME content type of file
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Name of the parameter
        /// </summary>
        public string Name { get; set; }
    }
}