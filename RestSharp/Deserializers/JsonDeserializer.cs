using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RestSharp.Extensions;
using SimpleJson;

namespace RestSharp.Deserializers
{
    using System.Xml;

    public class JsonDeserializer : IDeserializer
    {
        public JsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            object json = FindRoot(response.Content);

            return (T) ConvertValue(typeof(T).GetTypeInfo(), json);
        }

        private object FindRoot(string content)
        {
            object json = SimpleJson.SimpleJson.DeserializeObject(content);

            if (!RootElement.HasValue()) return json;

            if (!(json is IDictionary<string, object> dictionary)) return json;

            return dictionary.TryGetValue(RootElement, out var result) ? result : json;
        }

        private object Map(object target, IDictionary<string, object> data)
        {
            var objType = target.GetType().GetTypeInfo();
            var props = objType.GetProperties()
                .Where(p => p.CanWrite)
                .ToList();

            foreach (var prop in props)
            {
                string name;
                var type = prop.PropertyType.GetTypeInfo();
                var attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);

                if (attributes.Any())
                {
                    var attribute = (DeserializeAsAttribute) attributes.First();
                    name = attribute.Name;
                }
                else
                {
                    name = prop.Name;
                }

                object value = null;
                if (!data.TryGetValue(name, out value))
                {
                    string[] parts = name.Split('.');
                    IDictionary<string, object> currentData = data;

                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string actualName = parts[i].GetNameVariants(this.Culture)
                            .FirstOrDefault(currentData.ContainsKey);

                        if (actualName == null)
                        {
                            break;
                        }

                        if (i == parts.Length - 1)
                        {
                            value = currentData[actualName];
                        }
                        else
                        {
                            currentData = (IDictionary<string, object>) currentData[actualName];
                        }
                    }
                }

                if (value != null)
                {
                    prop.SetValue(target, ConvertValue(type, value), null);
                }
            }

            return target;
        }

        private IDictionary BuildDictionary(Type type, object parent)
        {
            IDictionary dict = (IDictionary) Activator.CreateInstance(type);
            Type keyType = type.GetTypeInfo().GetGenericArguments()[0];
            Type valueType = type.GetTypeInfo().GetGenericArguments()[1];

            foreach (KeyValuePair<string, object> child in (IDictionary<string, object>) parent)
            {
                object key = keyType != typeof(string)
                    ? Convert.ChangeType(child.Key, keyType, CultureInfo.InvariantCulture)
                    : child.Key;

                object item;

                if (valueType.GetTypeInfo().IsGenericType &&
                    valueType.GetTypeInfo().GetGenericTypeDefinition() == typeof(List<>))
                {
                    item = BuildList(valueType, child.Value);
                }
                else
                {
                    item = ConvertValue(valueType.GetTypeInfo(), child.Value);
                }

                dict.Add(key, item);
            }

            return dict;
        }

        private IList BuildList(Type type, object parent)
        {
            IList list = (IList) Activator.CreateInstance(type);
            Type listType = type.GetTypeInfo().GetInterfaces()
                .First
                    (x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            Type itemType = listType.GetTypeInfo().GetGenericArguments()[0];

            if (parent is IList list1)
            {
                foreach (object element in list1)
                {
                    if (itemType.GetTypeInfo().IsPrimitive)
                    {
                        object item = ConvertValue(itemType.GetTypeInfo(), element);

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

                        object item = ConvertValue(itemType.GetTypeInfo(), element);

                        list.Add(item);
                    }
                }
            }
            else
            {
                list.Add(ConvertValue(itemType.GetTypeInfo(), parent));
            }

            return list;
        }

        private object ConvertValue(TypeInfo typeInfo, object value)
        {
            string stringValue = Convert.ToString(value, Culture);

            // check for nullable and extract underlying type
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Since the type is nullable and no value is provided return null
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }

                typeInfo = typeInfo.GetGenericArguments()[0].GetTypeInfo();
            }

            if (typeInfo.AsType() == typeof(object))
            {
                if (value == null)
                {
                    return null;
                }
                typeInfo = value.GetType().GetTypeInfo();
            }

            var type = typeInfo.AsType();
            if (typeInfo.IsPrimitive)
            {
                return value.ChangeType(type, Culture);
            }

            if (typeInfo.IsEnum)
            {
                return type.FindEnumValue(stringValue, Culture);
            }

            if (type == typeof(Uri))
            {
                return new Uri(stringValue, UriKind.RelativeOrAbsolute);
            }

            if (type == typeof(string))
            {
                return stringValue;
            }

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                DateTime dt;

                if (DateFormat.HasValue())
                {
                    dt = DateTime.ParseExact(stringValue, DateFormat, Culture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                }
                else
                {
                    // try parsing instead
                    dt = stringValue.ParseJsonDate(this.Culture);
                }

                if (type == typeof(DateTime))
                {
                    return dt;
                }

                if (type == typeof(DateTimeOffset))
                {
                    return (DateTimeOffset) dt;
                }
            }
            else if (type == typeof(decimal))
            {
                if (value is double d)
                {
                    return (decimal) d;
                }

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
                Type genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(IEnumerable<>))
                {
                    Type itemType = typeInfo.GetGenericArguments()[0];
                    Type listType = typeof(List<>).MakeGenericType(itemType);
                    return BuildList(listType, value);
                }

                if (genericTypeDef == typeof(List<>))
                {
                    return BuildList(type, value);
                }

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    return BuildDictionary(type, value);
                }

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

        private object CreateAndMap(Type type, object element)
        {
            object instance = Activator.CreateInstance(type);

            Map(instance, (IDictionary<string, object>) element);

            return instance;
        }
    }
}