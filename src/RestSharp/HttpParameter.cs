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
using JetBrains.Annotations;

namespace RestSharp
{
    /// <summary>
    /// Representation of an HTTP parameter (QueryString or Form value)
    /// </summary>
    [PublicAPI]
    public class HttpParameter
    {
        /// <summary>
        /// Creates a new instance of HttpParameter
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Header value</param>
        /// <param name="contentType">Parameter content type</param>
        public HttpParameter(string name, string? value, string? contentType = null)
        {
            Name        = name;
            ContentType = contentType;
            Value       = value ?? "";
        }

        /// <summary>
        /// Creates a new instance of HttpParameter with value conversion
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Header value, which has to implement ToString() properly</param>
        /// <param name="contentType">Parameter content type</param>
        public HttpParameter(string name, object? value, string? contentType = null) : this(name, value?.ToString(), contentType) { }

        [Obsolete("Use parameterized constructor")]
        public HttpParameter() { }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Content-Type of the parameter
        /// </summary>
        public string? ContentType { get; set; }
    }
}
