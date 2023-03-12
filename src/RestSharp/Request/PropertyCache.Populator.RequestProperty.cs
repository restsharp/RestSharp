//  Copyright (c) .NET Foundation and Contributors
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

using System.Reflection;

namespace RestSharp;

static partial class PropertyCache<T> where T : class {
    sealed partial class Populator {
        sealed record RequestProperty {
            /// <summary>
            /// Gets or sets the <see cref="RequestPropertyAttribute.Name"/> associated
            /// with the property this object represents
            /// </summary>
            internal string Name { get; init; }
            /// <summary>
            /// Gets the <see cref="RequestPropertyAttribute.Format"/> associated with
            /// the property this object represents
            /// </summary>
            internal string? Format { get; }
            /// <summary>
            /// Gets the <see cref="RequestPropertyAttribute.ArrayQueryType"/> associated
            /// with the property this object represents
            /// </summary>
            internal RequestArrayQueryType ArrayQueryType { get; }
            /// <summary>
            /// Gets the return type of the property this object represents
            /// </summary>
            internal Type Type { get; }

            RequestProperty(string name, string? format, RequestArrayQueryType arrayQueryType, Type type) {
                Name           = name;
                Format         = format;
                ArrayQueryType = arrayQueryType;
                Type           = type;
            }

            /// <summary>
            /// Creates a new request property representation of the provided property
            /// </summary>
            /// <param name="property">The property to turn into a request property</param>
            /// <returns></returns>
            internal static RequestProperty From(PropertyInfo property) {
                var requestPropertyAttribute =
                    property.GetCustomAttribute<RequestPropertyAttribute>() ??
                    new RequestPropertyAttribute();

                var propertyName = requestPropertyAttribute.Name ?? property.Name;

                return new RequestProperty(
                    propertyName,
                    requestPropertyAttribute.Format,
                    requestPropertyAttribute.ArrayQueryType,
                    property.PropertyType
                );
            }
        }
    }
}
