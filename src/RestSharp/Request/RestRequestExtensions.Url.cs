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
        /// Adds a URL segment parameter to the request. The resource URL must have a placeholder for the parameter for it to work.
        /// For example, if you add a URL segment parameter with the name "id", the resource URL should contain {id} in its path.
        /// </summary>
        /// <param name="name">Name of the parameter; must be matching a placeholder in the resource URL as {name}</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="encode">Encode the value or not, default true</param>
        /// <returns></returns>
        public RestRequest AddUrlSegment(string name, string? value, bool encode = true)
            => request.AddParameter(new UrlSegmentParameter(name, value, encode));

        /// <summary>
        /// Adds a URL segment parameter to the request. The resource URL must have a placeholder for the parameter for it to work.
        /// For example, if you add a URL segment parameter with the name "id", the resource URL should contain {id} in its path.
        /// </summary>
        /// <param name="name">Name of the parameter; must be matching a placeholder in the resource URL as {name}</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="encode">Encode the value or not, default true</param>
        /// <returns></returns>
        public RestRequest AddUrlSegment<T>(string name, T value, bool encode = true) where T : struct
            => request.AddUrlSegment(name, value.ToString(), encode);
    }
}