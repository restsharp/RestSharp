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
// 

using System.Reflection;

namespace RestSharp;

static class ObjectParser {
    public static IEnumerable<(string Name, string? Value)> GetProperties(this object obj, params string[] includedProperties) {
        // automatically create parameters from object props
        var type  = obj.GetType();
        var props = type.GetProperties();

        var properties = new List<(string Name, string? Value)>();

        foreach (var prop in props.Where(x => IsAllowedProperty(x.Name))) {
            var val = prop.GetValue(obj, null);

            if (val == null) continue;

            if (prop.PropertyType.IsArray)
                properties.AddRange(GetArray(prop, val));
            else
                properties.Add(GetValue(prop, val));
        }

        string? ParseValue(string? format, object? value) => format == null ? value?.ToString() : string.Format($"{{0:{format}}}", value);

        IEnumerable<(string, string?)> GetArray(PropertyInfo propertyInfo, object? value) {
            var elementType = propertyInfo.PropertyType.GetElementType();
            var array       = (Array)value!;

            var attribute = propertyInfo.GetCustomAttribute<RequestPropertyAttribute>();
            var name      = attribute?.Name ?? propertyInfo.Name;

            var queryType = attribute?.ArrayQueryType ?? RequestArrayQueryType.CommaSeparated;

            if (array.Length > 0 && elementType != null) {
                // convert the array to an array of strings
                var values = array
                    .Cast<object>()
                    .Select(item => ParseValue(attribute?.Format, item));

                return queryType switch {
                    RequestArrayQueryType.CommaSeparated  => new (string, string?)[] { (name, string.Join(",", values)) },
                    RequestArrayQueryType.ArrayParameters => values.Select(x => ($"{name}[]", x)),
                    _                                     => throw new ArgumentOutOfRangeException()
                };
                    
            }

            return new (string, string?)[] { (name, null) };
        }

        (string, string?) GetValue(PropertyInfo propertyInfo, object? value) {
            var attribute = propertyInfo.GetCustomAttribute<RequestPropertyAttribute>();
            var name      = attribute?.Name ?? propertyInfo.Name;
            var val       = ParseValue(attribute?.Format, value);
            return (name, val);
        }

        bool IsAllowedProperty(string propertyName)
            => includedProperties.Length == 0 || includedProperties.Length > 0 && includedProperties.Contains(propertyName);

        return properties;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class RequestPropertyAttribute : Attribute {
    public string?               Name           { get; set; }
    public string?               Format         { get; set; }
    public RequestArrayQueryType ArrayQueryType { get; set; } = RequestArrayQueryType.CommaSeparated;
}

public enum RequestArrayQueryType { CommaSeparated, ArrayParameters }
