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
    ///     Container for files to be uploaded with requests
    /// </summary>
    public class FileParameter
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

        /// <summary>
        ///     Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="data">The data to use as the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <param name="contentType">The content type to use in the request.</param>
        /// <returns>The <see cref="FileParameter" /></returns>
        public static FileParameter Create(string name, byte[] data, string filename, string contentType)
            => new FileParameter
            {
                Writer        = s => s.Write(data, 0, data.Length),
                FileName      = filename,
                ContentType   = contentType,
                ContentLength = data.LongLength,
                Name          = name
            };

        /// <summary>
        ///     Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="data">The data to use as the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <returns>The <see cref="FileParameter" /> using the default content type.</returns>
        public static FileParameter Create(string name, byte[] data, string filename) => Create(name, data, filename, null);

        /// <summary>
        ///     Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="writer">Delegate that will be called with the request stream so you can write to it..</param>
        /// <param name="contentLength">The length of the data that will be written by te writer.</param>
        /// <param name="fileName">The filename to use in the request.</param>
        /// <param name="contentType">Optional: parameter content type</param>
        /// <returns>The <see cref="FileParameter" /> using the default content type.</returns>
        public static FileParameter Create(
            string name,
            Action<Stream> writer,
            long contentLength,
            string fileName,
            string contentType = null
        )
            => new FileParameter
            {
                Name          = name,
                FileName      = fileName,
                ContentType   = contentType,
                Writer        = writer,
                ContentLength = contentLength
            };
    }
}