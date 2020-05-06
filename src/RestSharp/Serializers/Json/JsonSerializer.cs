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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using RestSharp.Deserializers;
using RestSharp.Extensions;

namespace RestSharp.Serialization.Json
{
    public class JsonSerializer : IRestSerializer, IWithRootElement
    {
        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        ///     Serialize the object as JSON
        ///     If the object is already a serialized string returns it's value
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj)
            => IsSerializedString(obj, out var serializedString)
                ? serializedString
                : SimpleJson.SerializeObject(obj);

        /// <summary>
        ///     Content type for serialized content
        /// </summary>
        public string ContentType { get; set; } = Serialization.ContentType.Json;

        public string[] SupportedContentTypes { get; } = Serialization.ContentType.JsonAccept;

        public DataFormat DataFormat { get; } = DataFormat.Json;

        public string Serialize(Parameter parameter) => Serialize(parameter.Value);

        public T Deserialize<T>(IRestResponse response)
        {
            var json = FindRoot(response.Content);

            return (T) ConvertValue(typeof(T).GetTypeInfo(), json);
        }

        public string RootElement { get; set; }

        /// <summary>
        ///     Determines if the object is already a serialized string.
        /// </summary>
        static bool IsSerializedString(object obj, out string serializedString)
        {
            if (obj is string value)
            {
                var trimmed = value.Trim();

                if (trimmed.StartsWith("{")     && trimmed.EndsWith("}")
                    || trimmed.StartsWith("[{") && trimmed.EndsWith("}]"))
                {
                    serializedString = value;
                    return true;
                }
            }

            serializedString = null;
            return false;
        }

        object FindRoot(string content)
        {
            var json = SimpleJson.DeserializeObject(content);

            if (!RootElement.HasValue()) return json;

            if (!(json is IDictionary<string, object> dictionary)) return json;

            return dictionary.TryGetValue(RootElement, out var result) ? result : json;
        }

        object Map(object target, IDictionary<string, object> data)
        {
            var objType = target.GetType().GetTypeInfo();

            var props = objType.GetProperties()
                .Where(p => p.CanWrite)
                .ToList();

            foreach (var prop in props)
            {
                string name;
                var    attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);

                if (attributes.Any())
                {
                    var attribute = (DeserializeAsAttribute) attributes.First();
                    name = attribute.Name;
                }
                else
                {
                    name = prop.Name;
                }

                if (!data.TryGetValue(name, out var value))
                {
                    var parts       = name.Split('.');
                    var currentData = data;

                    for (var i = 0; i < parts.Length; ++i)
                    {
                        var variants = parts[i].GetNameVariants(Culture).Distinct();
                        var actualName = variants.FirstOrDefault(currentData.ContainsKey);

                        if (actualName == null) break;

                        if (i == parts.Length - 1)
                            value = currentData[actualName];
                        else
                            currentData = (IDictionary<string, object>) currentData[actualName];
                    }
                }

                if (value != null)
                {
                    var type = prop.PropertyType.GetTypeInfo();
                    prop.SetValue(target, ConvertValue(type, value), null);
                }
            }

            return target;
        }

        IDictionary BuildDictionary(Type type, object parent)
        {
            var dict      = (IDictionary) Activator.CreateInstance(type);
            var keyType   = type.GetTypeInfo().GetGenericArguments()[0];
            var valueType = type.GetTypeInfo().GetGenericArguments()[1];

            foreach (var child in (IDictionary<string, object>) parent)
            {
                var key = keyType != typeof(string)
                    ? Convert.ChangeType(child.Key, keyType, CultureInfo.InvariantCulture)
                    : child.Key;

                var item = valueType.GetTypeInfo().IsGenericType &&
                    valueType.GetTypeInfo().GetGenericTypeDefinition() == typeof(List<>)
                        ? BuildList(valueType, child.Value)
                        : ConvertValue(valueType.GetTypeInfo(), child.Value);

                dict.Add(key, item);
            }

            return dict;
        }

        IList BuildList(Type type, object parent)
        {
            var list = (IList) Activator.CreateInstance(type);

            var listType = type
                .GetTypeInfo()
                .GetInterfaces()
                .First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            var itemType = listType.GetTypeInfo().GetGenericArguments()[0];

            if (parent is IList list1)
                foreach (var element in list1)
                    if (itemType.GetTypeInfo().IsPrimitive)
                    {
                        var item = ConvertValue(itemType.GetTypeInfo(), element);

                        list.Add(item);
                    }
                    else if (itemType == typeof(string))
                    {
                        if (element == null)
                        {
                            list.Add(null);
                            continue;
                        }

                        list.Add(element.ToString());
                    }
                    else
                    {
                        if (element == null)
                        {
                            list.Add(null);
                            continue;
                        }

                        var item = ConvertValue(itemType.GetTypeInfo(), element);

                        list.Add(item);
                    }
            else
                list.Add(ConvertValue(itemType.GetTypeInfo(), parent));

            return list;
        }

        object ConvertValue(TypeInfo typeInfo, object value)
        {
            var stringValue = Convert.ToString(value, Culture);

            // check for nullable and extract underlying type
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Since the type is nullable and no value is provided return null
                if (stringValue.IsEmpty()) return null;

                typeInfo = typeInfo.GetGenericArguments()[0].GetTypeInfo();
            }

            if (typeInfo.AsType() == typeof(object))
            {
                if (value == null) return null;

                typeInfo = value.GetType().GetTypeInfo();
            }

            var type = typeInfo.AsType();
            if (typeInfo.IsPrimitive) return value.ChangeType(type);

            if (typeInfo.IsEnum) return type.FindEnumValue(stringValue, Culture);

            if (type == typeof(Uri)) return new Uri(stringValue, UriKind.RelativeOrAbsolute);

            if (type == typeof(string)) return stringValue;

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                DateTime dt;

                if (DateFormat.HasValue())
                    dt = DateTime.ParseExact(
                        stringValue, DateFormat, Culture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                    );
                else
                    // try parsing instead
                    dt = stringValue.ParseJsonDate(Culture);

                if (type == typeof(DateTime)) return dt;

                if (type == typeof(DateTimeOffset)) return (DateTimeOffset) dt;
            }
            else if (type == typeof(decimal))
            {
                if (value is double d) return (decimal) d;

                return stringValue.Contains("e")
                    ? decimal.Parse(stringValue, NumberStyles.Float, Culture)
                    : decimal.Parse(stringValue, Culture);
            }
            else if (type == typeof(Guid))
            {
                return string.IsNullOrEmpty(stringValue)
                    ? Guid.Empty
                    : new Guid(stringValue);
            }
            else if (type == typeof(TimeSpan))
            {
                // This should handle ISO 8601 durations
                return TimeSpan.TryParse(stringValue, out var timeSpan) ? timeSpan : XmlConvert.ToTimeSpan(stringValue);
            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(IEnumerable<>) || genericTypeDef == typeof(IList<>))
                {
                    var itemType = typeInfo.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(itemType);
                    return BuildList(listType, value);
                }

                if (genericTypeDef == typeof(List<>)) return BuildList(type, value);

                if (genericTypeDef == typeof(Dictionary<,>)) return BuildDictionary(type, value);

                // nested property classes
                return CreateAndMap(type, value);
            }
            else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
            {
                // handles classes that derive from List<T>
                return BuildList(type, value);
            }
            else if (type == typeof(JsonObject))
            {
                // simplify JsonObject into a Dictionary<string, object> 
                return BuildDictionary(typeof(Dictionary<string, object>), value);
            }
            else
            {
                // nested property classes
                return CreateAndMap(type, value);
            }

            return null;
        }

        object CreateAndMap(Type type, object element)
        {
            var instance = Activator.CreateInstance(type);

            return Map(instance, (IDictionary<string, object>) element);
        }
    }

    public class JsonDeserializer : JsonSerializer { }
}