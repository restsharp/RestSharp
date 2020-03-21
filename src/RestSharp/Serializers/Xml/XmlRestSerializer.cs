using System;
using System.Globalization;
using System.Text;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public class XmlRestSerializer : IRestSerializer, IXmlSerializer, IXmlDeserializer
    {
        public bool UseBytes { get; } = false;


        XmlSerilizationOptions _options         = XmlSerilizationOptions.Default;
        IXmlDeserializer       _xmlDeserializer = new XmlDeserializer();
        IXmlSerializer         _xmlSerializer   = new XmlSerializer();
        public string[] SupportedContentTypes { get; } = Serialization.ContentType.XmlAccept;

        public DataFormat DataFormat { get; } = DataFormat.Xml;

        public string ContentType { get; set; } = Serialization.ContentType.Xml;


        public T Deserialize<T>(IRestResponse response) => _xmlDeserializer.Deserialize<T>(response);

        public T Deserialize<T>(string payload) => _xmlDeserializer.Deserialize<T>(payload);


        public T DeserializeFromBytes<T>(byte[] payload)
            => throw new NotSupportedException("Deserializing XmlRest from byte[] array is not supported!");


        public string Serialize(object obj) => _xmlSerializer.Serialize(obj);

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


        public byte[] SerializeToBytes(object obj)
             => throw new System.NotSupportedException("Serializing XmlRest to byte[] array is not supported!");




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