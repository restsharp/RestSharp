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
using System.Globalization;
using System.Text;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public class XmlRestSerializer : IRestSerializer, IXmlSerializer, IXmlDeserializer
    {
        XmlSerilizationOptions _options         = XmlSerilizationOptions.Default;
        IXmlDeserializer       _xmlDeserializer = new XmlDeserializer();
        IXmlSerializer         _xmlSerializer   = new XmlSerializer();
        public string[] SupportedContentTypes { get; } = Serialization.ContentType.XmlAccept;

        public DataFormat DataFormat { get; } = DataFormat.Xml;

        public string ContentType { get; set; } = Serialization.ContentType.Xml;

        public string Serialize(object obj) => _xmlSerializer.Serialize(obj);

        public T Deserialize<T>(IRestResponse response) => _xmlDeserializer.Deserialize<T>(response);

        public string Serialize(Parameter parameter)
        {
            if (!(parameter is XmlParameter xmlParameter))
                throw new InvalidOperationException("Supplied parameter is not an XML parameter");

            var savedNamespace = _xmlSerializer.Namespace;
            _xmlSerializer.Namespace = xmlParameter.XmlNamespace ?? savedNamespace;

            var result = _xmlSerializer.Serialize(parameter.Value);

            _xmlSerializer.Namespace = savedNamespace;

            return result;
        }

        public string RootElement
        {
            get => _options.RootElement;
            set
            {
                _options.RootElement         = value;
                _xmlSerializer.RootElement   = value;
                _xmlDeserializer.RootElement = value;
            }
        }

        public string Namespace
        {
            get => _options.Namespace;
            set
            {
                _options.Namespace         = value;
                _xmlSerializer.Namespace   = value;
                _xmlDeserializer.Namespace = value;
            }
        }

        public string DateFormat
        {
            get => _options.DateFormat;
            set
            {
                _options.DateFormat         = value;
                _xmlSerializer.DateFormat   = value;
                _xmlDeserializer.DateFormat = value;
            }
        }

        public XmlRestSerializer WithOptions(XmlSerilizationOptions options)
        {
            _options = options;
            return this;
        }

        public XmlRestSerializer WithXmlSerializer<T>(XmlSerilizationOptions options = null)
            where T : IXmlSerializer, new()
        {
            if (options != null) _options = options;

            return WithXmlSerializer(
                new T
                {
                    Namespace   = _options.Namespace,
                    DateFormat  = _options.DateFormat,
                    RootElement = _options.RootElement
                }
            );
        }

        public XmlRestSerializer WithXmlSerializer(IXmlSerializer xmlSerializer)
        {
            _xmlSerializer = xmlSerializer;
            return this;
        }

        public XmlRestSerializer WithXmlDeserialzier<T>(XmlSerilizationOptions options = null)
            where T : IXmlDeserializer, new()
        {
            if (options != null) _options = options;

            return WithXmlDeserializer(
                new T
                {
                    Namespace   = _options.Namespace,
                    DateFormat  = _options.DateFormat,
                    RootElement = _options.RootElement
                }
            );
        }

        public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer)
        {
            _xmlDeserializer = xmlDeserializer;
            return this;
        }
    }

    public class XmlSerilizationOptions
    {
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

        public CultureInfo Culture { get; set; }

        public static XmlSerilizationOptions Default => new XmlSerilizationOptions
        {
            Culture = CultureInfo.InvariantCulture
        };
    }
}