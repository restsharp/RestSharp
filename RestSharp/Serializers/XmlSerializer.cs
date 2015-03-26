﻿#region License
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
using System.Globalization;
using System.Linq;
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
            ContentType = "text/xml";
        }

        /// <summary>
        /// Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public XmlSerializer(string @namespace)
        {
            Namespace = @namespace;
            ContentType = "text/xml";
        }

        /// <summary>
        /// Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML as string</returns>
        public string Serialize(object obj)
        {
            var doc = new XDocument();
            var t = obj.GetType();
            var name = t.Name;
            var options = t.GetAttribute<SerializeAsAttribute>();

            if (options != null)
            {
                name = options.TransformName(options.Name ?? name);
            }

            var root = new XElement(name.AsNamespaced(Namespace));

            if (obj is IList)
            {
                var itemTypeName = "";

                foreach (var item in (IList)obj)
                {
                    var type = item.GetType();
                    var opts = type.GetAttribute<SerializeAsAttribute>();

                    if (opts != null)
                    {
                        itemTypeName = opts.TransformName(opts.Name ?? name);
                    }

                    if (itemTypeName == "")
                    {
                        itemTypeName = type.Name;
                    }

                    var instance = new XElement(itemTypeName.AsNamespaced(Namespace));

                    Map(instance, item);
                    root.Add(instance);
                }
            }
            else
                Map(root, obj);

            if (RootElement.HasValue())
            {
                var wrapper = new XElement(RootElement.AsNamespaced(Namespace), root);
                doc.Add(wrapper);
            }
            else
            {
                doc.Add(root);
            }

            return doc.ToString();
        }

        private void Map(XElement root, object obj)
        {
            var objType = obj.GetType();
            var props = from p in objType.GetProperties()
                        let indexAttribute = p.GetAttribute<SerializeAsAttribute>()
                        where p.CanRead && p.CanWrite
                        orderby indexAttribute == null ? int.MaxValue : indexAttribute.Index
                        select p;
            var globalOptions = objType.GetAttribute<SerializeAsAttribute>();

            foreach (var prop in props)
            {
                var name = prop.Name;
                var rawValue = prop.GetValue(obj, null);

                if (rawValue == null)
                {
                    continue;
                }

                var value = GetSerializedValue(rawValue);
                var propType = prop.PropertyType;
                var useAttribute = false;
                var settings = prop.GetAttribute<SerializeAsAttribute>();

                if (settings != null)
                {
                    name = settings.Name.HasValue() ? settings.Name : name;
                    useAttribute = settings.Attribute;
                }

                var options = prop.GetAttribute<SerializeAsAttribute>();

                if (options != null)
                {
                    name = options.TransformName(name);
                }
                else if (globalOptions != null)
                {
                    name = globalOptions.TransformName(name);
                }

                var nsName = name.AsNamespaced(Namespace);
                var element = new XElement(nsName);

                if (propType.IsPrimitive || propType.IsValueType || propType == typeof(string))
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
                    var itemTypeName = "";

                    foreach (var item in (IList)rawValue)
                    {
                        if (itemTypeName == "")
                        {
                            var type = item.GetType();
                            var setting = type.GetAttribute<SerializeAsAttribute>();
                            itemTypeName = setting != null && setting.Name.HasValue()
                                ? setting.Name
                                : type.Name;
                        }

                        var instance = new XElement(itemTypeName.AsNamespaced(Namespace));

                        Map(instance, item);
                        element.Add(instance);
                    }
                }
                else
                {
                    Map(element, rawValue);
                }

                root.Add(element);
            }
        }

        private string GetSerializedValue(object obj)
        {
            var output = obj;

            if (obj is DateTime && DateFormat.HasValue())
            {
                output = ((DateTime)obj).ToString(DateFormat, CultureInfo.InvariantCulture);
            }

            if (obj is bool)
            {
                output = ((bool)obj).ToString(CultureInfo.InvariantCulture).ToLower();
            }

            if (IsNumeric(obj))
            {
                return SerializeNumber(obj);
            }

            return output.ToString();
        }

        static string SerializeNumber(object number)
        {
            if (number is long)
                return ((long)number).ToString(CultureInfo.InvariantCulture);

            if (number is ulong)
                return ((ulong)number).ToString(CultureInfo.InvariantCulture);

            if (number is int)
                return ((int)number).ToString(CultureInfo.InvariantCulture);

            if (number is uint)
                return ((uint)number).ToString(CultureInfo.InvariantCulture);

            if (number is decimal)
                return ((decimal)number).ToString(CultureInfo.InvariantCulture);

            if (number is float)
                return ((float)number).ToString(CultureInfo.InvariantCulture);

            return (Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, null, etc).
        /// </summary>
        static bool IsNumeric(object value)
        {
            if (value is sbyte)
                return true;

            if (value is byte)
                return true;

            if (value is short)
                return true;

            if (value is ushort)
                return true;

            if (value is int)
                return true;

            if (value is uint)
                return true;

            if (value is long)
                return true;

            if (value is ulong)
                return true;

            if (value is float)
                return true;

            if (value is double)
                return true;

            if (value is decimal)
                return true;

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
