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

public static class RestResponseExtensions {
    /// <param name="response">Response object</param>
    extension(RestResponse response) {
        /// <summary>
        /// Gets the value of the header with the specified name.
        /// </summary>
        /// <param name="headerName">Name of the header</param>
        /// <returns>Header value or null if the header is not found in the response</returns>
        public string? GetHeaderValue(string headerName)
            => response.Headers?.FirstOrDefault(x => NameIs(x.Name, headerName))?.Value.ToString();

        /// <summary>
        /// Gets all the values of the header with the specified name.
        /// </summary>
        /// <param name="headerName">Name of the header</param>
        /// <returns>Array of header values or empty array if the header is not found in the response</returns>
        public string[] GetHeaderValues(string headerName)
            => response.Headers
                    ?.Where(x => NameIs(x.Name, headerName))
                    .Select(x => x.Value.ToString() ?? "")
                    .ToArray() ??
                [];

        /// <summary>
        /// Gets the value of the content header with the specified name.
        /// </summary>
        /// <param name="headerName">Name of the header</param>
        /// <returns>Header value or null if the content header is not found in the response</returns>
        public string? GetContentHeaderValue(string headerName)
            => response.ContentHeaders?.FirstOrDefault(x => NameIs(x.Name, headerName))?.Value.ToString();

        /// <summary>
        /// Gets all the values of the content header with the specified name.
        /// </summary>
        /// <param name="headerName">Name of the header</param>
        /// <returns>Array of header values or empty array if the content header is not found in the response</returns>
        public string[] GetContentHeaderValues(string headerName)
            => response.ContentHeaders
                    ?.Where(x => NameIs(x.Name, headerName))
                    .Select(x => x.Value.ToString() ?? "")
                    .ToArray() ??
                [];
    }

    static bool NameIs(string? name, string headerName)
        => name != null && name.Equals(headerName, StringComparison.InvariantCultureIgnoreCase);
}