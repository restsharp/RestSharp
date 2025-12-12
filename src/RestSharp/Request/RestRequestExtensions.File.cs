// Copyright (c) .NET Foundation and Contributors
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

namespace RestSharp;

public static partial class RestRequestExtensions {
    /// <param name="request">Request instance</param>
    extension(RestRequest request) {
        /// <summary>
        /// Adds a file parameter to the request body. The file will be read from disk as a stream.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="path">Full path to the file</param>
        /// <param name="contentType">Optional: content type</param>
        /// <param name="options">File parameter header options</param>
        /// <returns></returns>
        public RestRequest AddFile(
            string                name,
            string                path,
            ContentType?          contentType = null,
            FileParameterOptions? options     = null
        )
            => request.AddFile(FileParameter.FromFile(path, name, contentType, options));

        /// <summary>
        /// Adds bytes to the request as file attachment
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="bytes">File content as bytes</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
        /// <param name="options">File parameter header options</param>
        /// <returns></returns>
        public RestRequest AddFile(
            string                name,
            byte[]                bytes,
            string                fileName,
            ContentType?          contentType = null,
            FileParameterOptions? options     = null
        )
            => request.AddFile(FileParameter.Create(name, bytes, fileName, contentType, options));

        /// <summary>
        /// Adds a file attachment to the request, where the file content will be retrieved from a given stream
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="getFile">Function that returns a stream with the file content</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
        /// <param name="options">File parameter header options</param>
        /// <returns></returns>
        public RestRequest AddFile(
            string                name,
            Func<Stream>          getFile,
            string                fileName,
            ContentType?          contentType = null,
            FileParameterOptions? options     = null
        )
            => request.AddFile(FileParameter.Create(name, getFile, fileName, contentType, options));

        internal void ValidateParameters() {
            if (!request.AlwaysSingleFileAsContent) return;

            var postParametersExists = request.Parameters.GetContentParameters(request.Method).Any();
            var bodyParametersExists = request.Parameters.Any(p => p.Type == ParameterType.RequestBody);

            if (request.AlwaysMultipartFormData)
                throw new ArgumentException("Failed to put file as content because flag AlwaysMultipartFormData is enabled");

            if (postParametersExists)
                throw new ArgumentException("Failed to put file as content because POST parameters were added");

            if (bodyParametersExists)
                throw new ArgumentException("Failed to put file as content because body parameters were added");
        }
    }
}