#region License

//   Copyright 2010 John Sheehan
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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers
{
    /// <summary>
    /// Default XML Serializer
    /// </summary>
    public class XmlSerializer : ISerializer
    {
        /// <summary>
        /// Default constructor, does not specify namespace
        /// </summary>
        public XmlSerializer()
        {
            this.ContentType = "text/xml";
        }

        /// <summary>
        /// Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public XmlSerializer(string @namespace)
        {
            this.Namespace = @namespace;
            this.ContentType = "text/xml";
        }

        /// <summary>
        /// Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML as string</returns>
        public string Serialize(object obj)
        {
            XDocument doc = new XDocument();
            Type t = obj.GetType();
            string name = t.Name;
            SerializeAsAttribute options = t.GetAttribute<SerializeAsAttribute>();

            if (options != null)
            {
                name = options.TransformName(options.Name ?? name);
            }

            XElement root = new XElement(name.AsNamespaced(this.Namespace));

            if (obj is IList)
            {
                string itemTypeName = "";

                foreach (object item in (IList) obj)
                {
                    Type type = item.GetType();
                    SerializeAsAttribute opts = type.GetAttribute<SerializeAsAttribute>();

                    if (opts != null)
                    {
                        itemTypeName = opts.TransformName(opts.Name ?? name);
                    }

                    if (itemTypeName == "")
                    {
                        itemTypeName = type.Name;
                    }

                    XElement instance = new XElement(itemTypeName.AsNamespaced(this.Namespace));

                    this.Map(instance, item);
                    root.Add(instance);
                }
            }
            else
            {
                this.Map(root, obj);
            }

            if (this.RootElement.HasValue())
            {
                XElement wrapper = new XElement(this.RootElement.AsNamespaced(this.Namespace), root);
                doc.Add(wrapper);
            }
            else
            {
                doc.Add(root);
            }

            return doc.ToString();
        }

        private void Map(XContainer root, object obj)
        {
            Type objType = obj.GetType();
            IEnumerable<PropertyInfo> props = from p in objType.GetProperties()
                                              let indexAttribute = p.GetAttribute<SerializeAsAttribute>()
                                              where p.CanRead && p.CanWrite
                                              orderby indexAttribute == null
                                                  ? int.MaxValue
                                                  : indexAttribute.Index
                                              select p;
            SerializeAsAttribute globalOptions = objType.GetAttribute<SerializeAsAttribute>();

            foreach (PropertyInfo prop in props)
            {
                string name = prop.Name;
                object rawValue = prop.GetValue(obj, null);

                if (rawValue == null)
                {
                    continue;
                }

                string value = this.GetSerializedValue(rawValue);
                Type propType = prop.PropertyType;
                bool useAttribute = false;
                SerializeAsAttribute settings = prop.GetAttribute<SerializeAsAttribute>();

                if (settings != null)
                {
                    name = settings.Name.HasValue()
                        ? settings.Name
                        : name;
                    useAttribute = settings.Attribute;
                }

                SerializeAsAttribute options = prop.GetAttribute<SerializeAsAttribute>();

                if (options != null)
                {
                    name = options.TransformName(name);
                }
                else if (globalOptions != null)
                {
                    name = globalOptions.TransformName(name);
                }

                XName nsName = name.AsNamespaced(this.Namespace);
                XElement element = new XElement(nsName);
#if !WINDOWS_UWP
                if (propType.IsPrimitive || propType.IsValueType || propType == typeof(string))
#else
                if (propType.GetTypeInfo().IsPrimitive || propType.GetTypeInfo().IsValueType || propType == typeof(string))
#endif
                {
                    if (useAttribute)
                    {
                        root.Add(new XAttribute(name, value));
                        continue;
                    }

                    element.Value = value;
                }
                else if (rawValue is IList)
                {
                    string itemTypeName = "";

                    foreach (object item in (IList) rawValue)
                    {
                        if (itemTypeName == "")
                        {
                            Type type = item.GetType();
                            SerializeAsAttribute setting = type.GetAttribute<SerializeAsAttribute>();

                            itemTypeName = setting != null && setting.Name.HasValue()
                                ? setting.Name
                                : type.Name;
                        }

                        XElement instance = new XElement(itemTypeName.AsNamespaced(this.Namespace));

                        this.Map(instance, item);
                        element.Add(instance);
                    }
                }
                else
                {
                    this.Map(element, rawValue);
                }

                root.Add(element);
            }
        }

        private string GetSerializedValue(object obj)
        {
            object output = obj;

            if (obj is DateTime && this.DateFormat.HasValue())
            {
                output = ((DateTime) obj).ToString(this.DateFormat, CultureInfo.InvariantCulture);
            }

            if (obj is bool)
            {
#if !WINDOWS_UWP
                output = ((bool) obj).ToString(CultureInfo.InvariantCulture).ToLower();
#else
                output = ((bool)obj).ToString().ToLowerInvariant();                
#endif
            }

            if (IsNumeric(obj))
            {
                return SerializeNumber(obj);
            }

            return output.ToString();
        }

        private static string SerializeNumber(object number)
        {
            if (number is long)
            {
                return ((long) number).ToString(CultureInfo.InvariantCulture);
            }

            if (number is ulong)
            {
                return ((ulong) number).ToString(CultureInfo.InvariantCulture);
            }

            if (number is int)
            {
                return ((int) number).ToString(CultureInfo.InvariantCulture);
            }

            if (number is uint)
            {
                return ((uint) number).ToString(CultureInfo.InvariantCulture);
            }

            if (number is decimal)
            {
                return ((decimal) number).ToString(CultureInfo.InvariantCulture);
            }

            if (number is float)
            {
                return ((float) number).ToString(CultureInfo.InvariantCulture);
            }

            return (Convert.ToDouble(number, CultureInfo.InvariantCulture)
                           .ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, null, etc).
        /// </summary>
        private static bool IsNumeric(object value)
        {
            if (value is sbyte)
            {
                return true;
            }

            if (value is byte)
            {
                return true;
            }

            if (value is short)
            {
                return true;
            }

            if (value is ushort)
            {
                return true;
            }

            if (value is int)
            {
                return true;
            }

            if (value is uint)
            {
                return true;
            }

            if (value is long)
            {
                return true;
            }

            if (value is ulong)
            {
                return true;
            }

            if (value is float)
            {
                return true;
            }

            if (value is double)
            {
                return true;
            }

            if (value is decimal)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        /// XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Format string to use when serializing dates
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }
}
