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

using System.Globalization;

namespace RestSharp.Serializers.Xml;

public class XmlRestSerializer : IRestSerializer, IXmlSerializer, IXmlDeserializer {
    XmlSerializationOptions _options = XmlSerializationOptions.Default;
    IXmlDeserializer        _xmlDeserializer;
    IXmlSerializer          _xmlSerializer;

    public XmlRestSerializer() : this(new DotNetXmlSerializer(), new DotNetXmlDeserializer()) { }

    public XmlRestSerializer(IXmlSerializer xmlSerializer, IXmlDeserializer xmlDeserializer) {
        _xmlDeserializer = xmlDeserializer;
        _xmlSerializer   = xmlSerializer;
    }

    public string[] SupportedContentTypes => Serializers.ContentType.XmlAccept;

    public DataFormat DataFormat => DataFormat.Xml;

    public string ContentType { get; set; } = Serializers.ContentType.Xml;

    public string? Serialize(object? obj) => _xmlSerializer.Serialize(Ensure.NotNull(obj, nameof(obj)));

    public T? Deserialize<T>(RestResponse response) => _xmlDeserializer.Deserialize<T>(response);

    public string? Serialize(Parameter parameter) {
        if (parameter is not XmlParameter xmlParameter)
            throw new ArgumentException("Supplied parameter is not an XML parameter", nameof(parameter));

        if (parameter.Value == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter value is null");

        var savedNamespace = _xmlSerializer.Namespace;
        _xmlSerializer.Namespace = xmlParameter.XmlNamespace ?? savedNamespace;

        var result = _xmlSerializer.Serialize(parameter.Value);

        _xmlSerializer.Namespace = savedNamespace;

        return result;
    }

    public string? RootElement {
        get => _options.RootElement;
        set {
            _options.RootElement         = value;
            _xmlSerializer.RootElement   = value;
            _xmlDeserializer.RootElement = value;
        }
    }

    public string? Namespace {
        get => _options.Namespace;
        set {
            var ns = Ensure.NotEmptyString(value, nameof(Namespace));
            _options.Namespace         = ns;
            _xmlSerializer.Namespace   = ns;
            _xmlDeserializer.Namespace = ns;
        }
    }

    public string? DateFormat {
        get => _options.DateFormat;
        set {
            var dateFormat = Ensure.NotEmptyString(value, nameof(DataFormat));
            _options.DateFormat         = dateFormat;
            _xmlSerializer.DateFormat   = dateFormat;
            _xmlDeserializer.DateFormat = dateFormat;
        }
    }

    public XmlRestSerializer WithOptions(XmlSerializationOptions options) {
        _options = options;
        return this;
    }

    public XmlRestSerializer WithXmlSerializer<T>(XmlSerializationOptions? options = null) where T : IXmlSerializer, new() {
        if (options != null) _options = options;

        return WithXmlSerializer(
            new T {
                Namespace   = _options.Namespace,
                DateFormat  = _options.DateFormat,
                RootElement = _options.RootElement
            }
        );
    }

    public XmlRestSerializer WithXmlSerializer(IXmlSerializer xmlSerializer) {
        _xmlSerializer = xmlSerializer;
        return this;
    }

    public XmlRestSerializer WithXmlDeserializer<T>(XmlSerializationOptions? options = null) where T : IXmlDeserializer, new() {
        if (options != null) _options = options;

        return WithXmlDeserializer(
            new T {
                Namespace   = _options.Namespace,
                DateFormat  = _options.DateFormat,
                RootElement = _options.RootElement
            }
        );
    }

    public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer) {
        _xmlDeserializer = xmlDeserializer;
        return this;
    }
}

public class XmlSerializationOptions {
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

    public CultureInfo? Culture { get; set; }

    public static XmlSerializationOptions Default => new() { Culture = CultureInfo.InvariantCulture };
}