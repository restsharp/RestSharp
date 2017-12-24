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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers
{
    /// <summary>
    ///     Default XML Serializer
    /// </summary>
    public class XmlSerializer : ISerializer
    {
        /// <summary>
        ///     Default constructor, does not specify namespace
        /// </summary>
        public XmlSerializer()
        {
            ContentType = "text/xml";
        }

        /// <summary>
        ///     Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public XmlSerializer(string @namespace)
        {
            Namespace = @namespace;
            ContentType = "text/xml";
        }

        /// <summary>
        ///     Serialize the object as XML
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
                name = options.TransformName(options.Name ?? name);

            var root = new XElement(name.AsNamespaced(Namespace));

            if (obj is IList list)
            {
                var itemTypeName = "";

                foreach (var item in list)
                {
                    var type = item.GetType();
                    var opts = type.GetAttribute<SerializeAsAttribute>();

                    if (opts != null)
                        itemTypeName = opts.TransformName(opts.Name ?? name);

                    if (itemTypeName == "")
                        itemTypeName = type.Name;

                    var instance = new XElement(itemTypeName.AsNamespaced(Namespace));

                    Map(instance, item);
                    root.Add(instance);
                }
            }
            else
            {
                Map(root, obj);
            }

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

        /// <summary>
        ///     Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        ///     XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        ///     Format string to use when serializing dates
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        ///     Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }

        private void Map(XContainer root, object obj)
        {
            Type objType = obj.GetType();
            var props = from p in objType.GetProperties()
                                              let indexAttribute = p.GetAttribute<SerializeAsAttribute>()
                                              where p.CanRead && p.CanWrite
                                              orderby indexAttribute?.Index ?? int.MaxValue
                                              select p;
            SerializeAsAttribute globalOptions = objType.GetAttribute<SerializeAsAttribute>();
            bool textContentAttributeAlreadyUsed = false;

            foreach (var prop in props)
            {
                var name = prop.Name;
                var rawValue = prop.GetValue(obj, null);

                if (rawValue == null)
                    continue;

                string value = this.GetSerializedValue(rawValue);
                Type propType = prop.PropertyType;
                bool useAttribute = false;
                bool setTextContent = false;
                SerializeAsAttribute options = prop.GetAttribute<SerializeAsAttribute>();

                if (options != null)
                {
                    name = options.Name.HasValue()
                        ? options.Name
                        : name;

                    name = options.TransformName(name);

                    useAttribute = options.Attribute;

                    setTextContent = options.Content;

                    if (textContentAttributeAlreadyUsed && setTextContent)
                    {
                        throw new ArgumentException("Class cannot have two properties marked with " +
                            "SerializeAs(Content = true) attribute.");
                    }

                    textContentAttributeAlreadyUsed |= setTextContent;
                }
                else if (globalOptions != null)
                    name = globalOptions.TransformName(name);

                var nsName = name.AsNamespaced(Namespace);
                var element = new XElement(nsName);
                if (propType.GetTypeInfo().IsPrimitive || propType.GetTypeInfo().IsValueType ||
                    propType == typeof(string))
                {
                    if (useAttribute)
                    {
                        root.Add(new XAttribute(name, value));
                        continue;
                    } else if (setTextContent)
                    {
                        root.Add(new XText(value));
                        continue;
                    }

                    element.Value = value;
                }
                else if (rawValue is IList)
                {
                    var itemTypeName = "";

                    foreach (var item in (IList) rawValue)
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

            switch (obj)
            {
                case DateTime time when DateFormat.HasValue():
                    output = time.ToString(DateFormat, CultureInfo.InvariantCulture);
                    break;
                case bool _:
                    output = ((bool) obj).ToString().ToLowerInvariant();
                    break;
            }

            return IsNumeric(obj) ? SerializeNumber(obj) : output.ToString();
        }

        private static string SerializeNumber(object number)
        {
            switch (number)
            {
                case long l:
                    return l.ToString(CultureInfo.InvariantCulture);
                case ulong @ulong:
                    return @ulong.ToString(CultureInfo.InvariantCulture);
                case int i:
                    return i.ToString(CultureInfo.InvariantCulture);
                case uint u:
                    return u.ToString(CultureInfo.InvariantCulture);
                case decimal @decimal:
                    return @decimal.ToString(CultureInfo.InvariantCulture);
                case float f:
                    return f.ToString(CultureInfo.InvariantCulture);
            }

            return Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Determines if a given object is numeric in any way
        ///     (can be integer, double, null, etc).
        /// </summary>
        private static bool IsNumeric(object value)
        {
            switch (value)
            {
                case sbyte _:
                    return true;
                case byte _:
                    return true;
                case short _:
                    return true;
                case ushort _:
                    return true;
                case int _:
                    return true;
                case uint _:
                    return true;
                case long _:
                    return true;
                case ulong _:
                    return true;
                case float _:
                    return true;
                case double _:
                    return true;
                case decimal _:
                    return true;
            }

            return false;
        }
    }
}