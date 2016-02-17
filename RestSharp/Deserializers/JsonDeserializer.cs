using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RestSharp.Extensions;

namespace RestSharp.Deserializers
{
    using System.Xml;

    public class JsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; }

        public JsonDeserializer()
        {
            this.Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            object json = this.FindRoot(response.Content);

            return (T)this.ConvertValue(typeof(T), json);
        }

        private object FindRoot(string content)
        {
            object json = SimpleJson.DeserializeObject(content);

            if (this.RootElement.HasValue())
            {
                IDictionary<string, object> dictionary = json as IDictionary<string, object>;

                if (dictionary != null)
                {
                    object result;
                    if (dictionary.TryGetValue(this.RootElement, out result))
                    {
                        return result;
                    }
                }
            }

            return json;
        }

        private object Map(object target, IDictionary<string, object> data)
        {
            Type objType = target.GetType();
            List<PropertyInfo> props = objType.GetProperties()
                                              .Where(p => p.CanWrite)
                                              .ToList();

            foreach (PropertyInfo prop in props)
            {
                string name;
                Type type = prop.PropertyType;
#if !WINDOWS_UWP
                object[] attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);               
                
                if (attributes.Length > 0)
#else
                IEnumerable<Attribute> attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);

                if (attributes.Count() > 0)
#endif
                {                    
                    DeserializeAsAttribute attribute = (DeserializeAsAttribute)attributes.First();
                    name = attribute.Name;
                }
                else
                {
                    name = prop.Name;
                }

                string[] parts = name.Split('.');
                IDictionary<string, object> currentData = data;
                object value = null;

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

                if (value != null)
                {
                    prop.SetValue(target, this.ConvertValue(type, value), null);
                }
            }

            return target;
        }

        private IDictionary BuildDictionary(Type type, object parent)
        {
            IDictionary dict = (IDictionary) Activator.CreateInstance(type);
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            foreach (KeyValuePair<string, object> child in (IDictionary<string, object>) parent)
            {
                object key = keyType != typeof(string)
                    ? Convert.ChangeType(child.Key, keyType, CultureInfo.InvariantCulture)
                    : child.Key;

                object item;

#if !WINDOWS_UWP
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
#else
                
                if (valueType.GetTypeInfo().IsGenericType && valueType.GetTypeInfo().GetGenericTypeDefinition() == typeof(List<>))    
#endif
                {
                    item = this.BuildList(valueType, child.Value);
                }
                else
                {
                    item = this.ConvertValue(valueType, child.Value);
                }

                dict.Add(key, item);
            }

            return dict;
        }

        private IList BuildList(Type type, object parent)
        {
            IList list = (IList) Activator.CreateInstance(type);
            Type listType = type.GetInterfaces()
                                .First
#if !WINDOWS_UWP
                (x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
#else
                (x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
#endif
            Type itemType = listType.GetGenericArguments()[0];

            if (parent is IList)
            {
                foreach (object element in (IList) parent)
                {
#if !WINDOWS_UWP
                    if (itemType.IsPrimitive)
#else
                    if (itemType.GetTypeInfo().IsPrimitive)
#endif
                    {
                        object item = this.ConvertValue(itemType, element);

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

                        object item = this.ConvertValue(itemType, element);

                        list.Add(item);
                    }
                }
            }
            else
            {
                list.Add(this.ConvertValue(itemType, parent));
            }

            return list;
        }

        private object ConvertValue(Type type, object value)
        {
            string stringValue = Convert.ToString(value, this.Culture);

            // check for nullable and extract underlying type
#if !WINDOWS_UWP
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
#else
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
#endif
            {
                // Since the type is nullable and no value is provided return null
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }

                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(object))
            {
                if (value == null)
                {
                    return null;
                }
                type = value.GetType();
            }

#if !WINDOWS_UWP
            if (type.IsPrimitive)
            {
                return value.ChangeType(type, this.Culture);
            }

            if (type.IsEnum)
            {
                return type.FindEnumValue(stringValue, this.Culture);
            }
#else
            if (type.GetTypeInfo().IsPrimitive)
            {
                return value.ChangeType(type, this.Culture);
            }

            if (type.GetTypeInfo().IsEnum)
            {
                return type.FindEnumValue(stringValue, this.Culture);
            }
#endif

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

                if (this.DateFormat.HasValue())
                {
                    dt = DateTime.ParseExact(stringValue, this.DateFormat, this.Culture,
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
                if (value is double)
                {
                    return (decimal) ((double) value);
                }

                if (stringValue.Contains("e"))
                {
                    return decimal.Parse(stringValue, NumberStyles.Float, this.Culture);
                }

                return decimal.Parse(stringValue, this.Culture);
            }
            else if (type == typeof(Guid))
            {
                return string.IsNullOrEmpty(stringValue)
                    ? Guid.Empty
                    : new Guid(stringValue);
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpan timeSpan;

                if (TimeSpan.TryParse(stringValue, out timeSpan))
                {
                    return timeSpan;
                }

                // This should handle ISO 8601 durations
                return XmlConvert.ToTimeSpan(stringValue);
            }
#if !WINDOWS_UWP
            else if (type.IsGenericType)
#else
            else if (type.GetTypeInfo().IsGenericType)
#endif
            {
                Type genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(IEnumerable<>))
                {
                    Type itemType = type.GetGenericArguments()[0];
                    Type listType = typeof(List<>).MakeGenericType(itemType);
                    return this.BuildList(listType, value);
                }

                if (genericTypeDef == typeof(List<>))
                {
                    return this.BuildList(type, value);
                }

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    return this.BuildDictionary(type, value);
                }

                // nested property classes
                return this.CreateAndMap(type, value);
            }
            else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
            {
                // handles classes that derive from List<T>
                return this.BuildList(type, value);
            }
            else if (type == typeof(JsonObject))
            {
                // simplify JsonObject into a Dictionary<string, object> 
                return this.BuildDictionary(typeof(Dictionary<string, object>), value);
            }
            else
            {
                // nested property classes
                return this.CreateAndMap(type, value);
            }

            return null;
        }

        private object CreateAndMap(Type type, object element)
        {
            object instance = Activator.CreateInstance(type);

            this.Map(instance, (IDictionary<string, object>) element);

            return instance;
        }
    }
}
