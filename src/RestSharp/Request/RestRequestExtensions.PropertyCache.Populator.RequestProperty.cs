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

public static partial class RestRequestExtensions {
    static partial class PropertyCache<T> where T : class {
        sealed partial class Populator {
            sealed record RequestProperty {
                internal string Name { get; init; }
                internal string? Format { get; }
                internal RequestArrayQueryType ArrayQueryType { get; }
                internal Type Type { get; }

                private RequestProperty(string name, string? format, RequestArrayQueryType arrayQueryType, Type type) {
                    Name = name;
                    Format = format;
                    ArrayQueryType = arrayQueryType;
                    Type = type;
                }

                internal static RequestProperty From(PropertyInfo property) {
                    var requestPropertyAttribute =
                        property.GetCustomAttribute<RequestPropertyAttribute>() ??
                        new RequestPropertyAttribute();

                    var propertyName = requestPropertyAttribute.Name ?? property.Name;

                    return new RequestProperty(
                        propertyName,
                        requestPropertyAttribute.Format,
                        requestPropertyAttribute.ArrayQueryType,
                        property.PropertyType);
                }
            }

        }
    }
}
