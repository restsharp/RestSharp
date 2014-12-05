﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RestSharp.Extensions;

namespace RestSharp.Deserializers
{
    public class JsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; }

        public JsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var target = Activator.CreateInstance<T>();

            if (target is IList)
            {
                var objType = target.GetType();

                if (RootElement.HasValue())
                {
                    var root = FindRoot(response.Content);
                    target = (T)BuildList(objType, root);
                }
                else
                {
                    var data = SimpleJson.DeserializeObject(response.Content);
                    target = (T)BuildList(objType, data);
                }
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(response.Content);
                target = (T)BuildDictionary(target.GetType(), root);
            }
            else
            {
                var root = FindRoot(response.Content);
                target = (T)Map(target, (IDictionary<string, object>)root);
            }

            return target;
        }

        private object FindRoot(string content)
        {
            var data = (IDictionary<string, object>)SimpleJson.DeserializeObject(content);

            if (RootElement.HasValue() && data.ContainsKey(RootElement))
            {
                return data[RootElement];
            }

            return data;
        }

        private object Map(object target, IDictionary<string, object> data)
        {
            var objType = target.GetType();
            var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;
                var attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);
                string name;

	            var firstAttribute = attributes.FirstOrDefault();
				if (firstAttribute != null)
                {
                    var attribute = (DeserializeAsAttribute)firstAttribute;
                    name = attribute.Name;
                }
                else
                {
                    name = prop.Name;
                }

                var parts = name.Split('.');
                var currentData = data;
                object value = null;

                for (var i = 0; i < parts.Length; ++i)
                {
                    var actualName = parts[i].GetNameVariants(Culture).FirstOrDefault(currentData.ContainsKey);

                    if (actualName == null)
                        break;

                    if (i == parts.Length - 1)
                        value = currentData[actualName];
                    else
                        currentData = (IDictionary<string, object>)currentData[actualName];
                }

                if (value != null)
                    prop.SetValue(target, ConvertValue(type, value), null);
            }

            return target;
        }

        private IDictionary BuildDictionary(Type type, object parent)
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];

            foreach (var child in (IDictionary<string, object>)parent)
            {
                var key = child.Key;
                object item = null;

                if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    item = BuildList(valueType, child.Value);
                }
                else
                {
                    item = ConvertValue(valueType, child.Value);
                }

                dict.Add(key, item);
            }

            return dict;
        }

        private IList BuildList(Type type, object parent)
        {
            var list = (IList)Activator.CreateInstance(type);
            var listType = type.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            var itemType = listType.GetGenericArguments()[0];

            if (parent is IList)
            {
                foreach (var element in (IList)parent)
                {
                    if (itemType.GetTypeInfo().IsPrimitive)
                    {
                        var item = ConvertValue(itemType, element);
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

                        var item = ConvertValue(itemType, element);
                        list.Add(item);
                    }
                }
            }
            else
            {
                list.Add(ConvertValue(itemType, parent));
            }

            return list;
        }

        private object ConvertValue(Type type, object value)
        {
            var stringValue = Convert.ToString(value, Culture);

            // check for nullable and extract underlying type
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Since the type is nullable and no value is provided return null
                if (String.IsNullOrEmpty(stringValue))
                    return null;

                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(System.Object) && value != null)
            {
                type = value.GetType();
            }

            if (type.GetTypeInfo().IsPrimitive)
            {
                return value.ChangeType(type, Culture);
            }

            if (type.GetTypeInfo().IsEnum)
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

            if (type == typeof(DateTime)
#if !PocketPC
 || type == typeof(DateTimeOffset)
#endif
)
            {
                DateTime dt;

                if (DateFormat.HasValue())
                {
                    dt = DateTime.ParseExact(stringValue, DateFormat, Culture);
                }
                else
                {
                    // try parsing instead
                    dt = stringValue.ParseJsonDate(Culture);
                }

#if PocketPC
                return dt;
#else
                if (type == typeof(DateTime))
                {
                    return dt;
                }

                if (type == typeof(DateTimeOffset))
                {
                    return (DateTimeOffset)dt;
                }
#endif
            }
            else if (type == typeof(Decimal))
            {
                if (value is double)
                    return (decimal)((double)value);

                return Decimal.Parse(stringValue, Culture);
            }
            else if (type == typeof(Guid))
            {
                return string.IsNullOrEmpty(stringValue) ? Guid.Empty : new Guid(stringValue);
            }
            else if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(stringValue);
            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(List<>))
                {
                    return BuildList(type, value);
                }

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    var keyType = type.GetGenericArguments()[0];

                    // only supports Dict<string, T>()
                    if (keyType == typeof(string))
                    {
                        return BuildDictionary(type, value);
                    }
                }
                else
                {
                    // nested property classes
                    return CreateAndMap(type, value);
                }
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
            var instance = Activator.CreateInstance(type);

            Map(instance, (IDictionary<string, object>)element);

            return instance;
        }
    }
}
