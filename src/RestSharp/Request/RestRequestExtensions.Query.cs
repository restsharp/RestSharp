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
        /// Adds a query string parameter to the request. The request resource should not contain any placeholders for this parameter.
        /// The parameter will be added to the request URL as a query string using name=value format.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="encode">Encode the value or not, default true</param>
        /// <returns></returns>
        public RestRequest AddQueryParameter(string name, string? value, bool encode = true)
            => request.AddParameter(new QueryParameter(name, value, encode));

        /// <summary>
        /// Adds a query string parameter to the request. The request resource should not contain any placeholders for this parameter.
        /// The parameter will be added to the request URL as a query string using name=value format.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="encode">Encode the value or not, default true</param>
        /// <returns></returns>
        public RestRequest AddQueryParameter<T>(string name, T value, bool encode = true) where T : struct
            => request.AddQueryParameter(name, value.ToString(), encode);
    }
}