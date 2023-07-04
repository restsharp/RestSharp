//   Copyright (c) .NET Foundation and Contributors
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers.Xml;

/// <summary>
/// Default XML Serializer
/// </summary>
public class XmlSerializer : IXmlSerializer, IWithRootElement, IWithDateFormat {
    /// <summary>
    /// Default constructor, does not specify namespace
    /// </summary>
    public XmlSerializer() { }

    /// <summary>
    /// Specify the namespaced to be used when serializing
    /// </summary>
    /// <param name="namespace">XML namespace</param>
    public XmlSerializer(string @namespace) => Namespace = @namespace;

    /// <summary>
    /// Serialize the object as XML
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>XML as string</returns>
    public string Serialize(object obj) {
        var doc     = new XDocument();
        var t       = obj.GetType();
        var name    = t.Name;
        var options = t.GetAttribute<SerializeAsAttribute>();

        if (options != null)
            name = options.TransformName(options.Name ?? name);

        var root = new XElement(name.AsNamespaced(Namespace)!);

        if (obj is IList list) {
            var itemTypeName = "";

            foreach (var item in list) {
                var type = item.GetType();
                var opts = type.GetAttribute<SerializeAsAttribute>();

                if (opts != null)
                    itemTypeName = opts.TransformName(opts.Name ?? name);

                if (itemTypeName == "")
                    itemTypeName = type.Name;

                var instance = new XElement(itemTypeName.AsNamespaced(Namespace)!);

                Map(instance, item);
                root.Add(instance);
            }
        }
        else {
            Map(root, obj);
        }

        if (RootElement != null) {
            var wrapper = new XElement(RootElement.AsNamespaced(Namespace)!, root);
            doc.Add(wrapper);
        }
        else {
            doc.Add(root);
        }

        return doc.ToString();
    }

    /// <summary>
    /// Name of the root element to use when serializing
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// XML namespace to use when serializing
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Format string to use when serializing dates
    /// </summary>
    public string? DateFormat { get; set; }

    /// <summary>
    /// Content type for serialized content
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Xml;

    void Map(XContainer root, object obj) {
        var objType = obj.GetType();

        var props = objType.GetProperties()
            .Select(p => new { p, indexAttribute = p.GetAttribute<SerializeAsAttribute>() })
            .Where(t => t.p is { CanRead: true, CanWrite: true })
            .OrderBy(t => t.indexAttribute?.Index ?? int.MaxValue)
            .Select(t => t.p);
        var globalOptions                   = objType.GetAttribute<SerializeAsAttribute>();
        var textContentAttributeAlreadyUsed = false;

        foreach (var prop in props) {
            var name     = prop.Name;
            var rawValue = prop.GetValue(obj, null);

            if (rawValue == null)
                continue;

            var propType       = prop.PropertyType;
            var useAttribute   = false;
            var setTextContent = false;
            var options        = prop.GetAttribute<SerializeAsAttribute>();

            if (options != null) {
                name = options.Name.IsNotEmpty()
                    ? options.Name
                    : name;

                name = options.TransformName(name!);

                useAttribute = options.Attribute;

                setTextContent = options.Content;

                if (textContentAttributeAlreadyUsed && setTextContent)
                    throw new ArgumentException("Class cannot have two properties marked with SerializeAs(Content = true) attribute.");

                textContentAttributeAlreadyUsed |= setTextContent;
            }
            else if (globalOptions != null) {
                name = globalOptions.TransformName(name);
            }

            var nsName  = name.AsNamespaced(Namespace);
            var element = new XElement(nsName!);

            if (propType.GetTypeInfo().IsPrimitive ||
                propType.GetTypeInfo().IsValueType ||
                propType == typeof(string)) {
                var value = GetSerializedValue(rawValue);

                if (useAttribute) {
                    root.Add(new XAttribute(name, value));
                    continue;
                }

                if (setTextContent) {
                    root.Add(new XText(value));
                    continue;
                }

                element.Value = value;
            }
            else if (rawValue is IList items) {
                foreach (var item in items) {
                    var type    = item.GetType();
                    var setting = type.GetAttribute<SerializeAsAttribute>();

                    var itemTypeName = setting != null && setting.Name.IsNotEmpty()
                        ? setting.Name
                        : type.Name;

                    var instance = new XElement(itemTypeName.AsNamespaced(Namespace)!);

                    Map(instance, item);

                    if (setTextContent) {
                        root.Add(instance);
                    }
                    else {
                        element.Add(instance);
                    }
                }

                if (setTextContent) {
                    continue;
                }
            }
            else {
                Map(element, rawValue);
            }

            root.Add(element);
        }
    }

    string GetSerializedValue(object obj) {
        var output = obj switch {
            DateTime time when DateFormat.IsNotEmpty() => time.ToString(DateFormat, CultureInfo.InvariantCulture),
            bool b                                     => b.ToString().ToLowerInvariant(),
            _                                          => obj
        };

        return IsNumeric(obj) ? SerializeNumber(obj) : output.ToString()!;
    }

    static string SerializeNumber(object number)
        => number switch {
            long l           => l.ToString(CultureInfo.InvariantCulture),
            ulong @ulong     => @ulong.ToString(CultureInfo.InvariantCulture),
            int i            => i.ToString(CultureInfo.InvariantCulture),
            uint u           => u.ToString(CultureInfo.InvariantCulture),
            decimal @decimal => @decimal.ToString(CultureInfo.InvariantCulture),
            float f          => f.ToString(CultureInfo.InvariantCulture),
            _                => Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture)
        };

    /// <summary>
    /// Determines if a given object is numeric in any way
    /// (can be integer, double, null, etc).
    /// </summary>
    static bool IsNumeric(object value)
        => value switch {
            sbyte   => true,
            byte    => true,
            short   => true,
            ushort  => true,
            int     => true,
            uint    => true,
            long    => true,
            ulong   => true,
            float   => true,
            double  => true,
            decimal => true,
            _       => false
        };
}