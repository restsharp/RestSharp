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
        /// Gets object properties and adds each property as a form data parameter
        /// </summary>
        /// <param name="obj">Object to add as form data</param>
        /// <param name="includedProperties">Properties to include, or nothing to include everything. The array will be sorted.</param>
        /// <returns></returns>
        public RestRequest AddObject<T>(T obj, params string[] includedProperties) where T : class {
            var props = obj.GetProperties(includedProperties);

            foreach (var prop in props) {
                request.AddParameter(prop.Name, prop.Value, prop.Encode);
            }

            return request;
        }

        /// <summary>
        /// Gets object properties and adds each property as a form data parameter
        /// </summary>
        /// <remarks>
        /// This method gets public instance properties from the provided <typeparamref name="T"/> type
        /// rather than from <paramref name="obj"/> itself, which allows for caching of properties and
        /// other optimizations. If you don't know the type at runtime, or wish to use properties not
        /// available from the provided type parameter, consider using <see cref="AddObject{T}(RestRequest, T, string[])"/>
        /// </remarks>
        /// <param name="obj">Object to add as form data</param>
        /// <param name="includedProperties">Properties to include, or nothing to include everything. The array will be sorted.</param>
        /// <returns></returns>
        public RestRequest AddObjectStatic<T>(T obj, params string[] includedProperties) where T : class
            => request.AddParameters(PropertyCache<T>.GetParameters(obj, includedProperties));

        /// <summary>
        /// Gets object properties and adds each property as a form data parameter
        /// </summary>
        /// <remarks>
        /// This method gets public instance properties from the provided <typeparamref name="T"/> type
        /// rather than from <paramref name="obj"/> itself, which allows for caching of properties and
        /// other optimizations. If you don't know the type at runtime, or wish to use properties not
        /// available from the provided type parameter, consider using <see cref="AddObject{T}(RestRequest, T, string[])"/>
        /// </remarks>
        /// <param name="obj">Object to add as form data</param>
        /// <returns></returns>
        public RestRequest AddObjectStatic<T>(T obj) where T : class
            => request.AddParameters(PropertyCache<T>.GetParameters(obj));
    }
}