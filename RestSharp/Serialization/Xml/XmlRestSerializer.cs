using System.Globalization;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public class XmlRestSerializer : IRestSerializer, IXmlSerializer, IXmlDeserializer
    {
        public string[] SupportedContentTypes { get; } =
        {
            "application/xml", "text/xml", "*+xml", "*"
        };

        public DataFormat DataFormat { get; } = DataFormat.Xml;

        public string ContentType { get; set; } = "text/xml";

        public string Serialize(object obj) => _xmlSerializer.Serialize(obj);

        public T Deserialize<T>(IRestResponse response) => _xmlDeserializer.Deserialize<T>(response);

        public XmlRestSerializer WithOptions(XmlSerilizationOptions options)
        {
            _options = options;
            return this;
        }

        public XmlRestSerializer WithXmlSerializer<T>(XmlSerilizationOptions options = null)
            where T : IXmlSerializer, new()
        {
            if (options != null) _options = options;

            return WithXmlSerializer(new T
            {
                Namespace = _options.Namespace,
                DateFormat = _options.DateFormat,
                RootElement = _options.RootElement
            });
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

            return WithXmlDeserializer(new T
            {
                Namespace = _options.Namespace,
                DateFormat = _options.DateFormat,
                RootElement = _options.RootElement
            });
        }

        public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer)
        {
            _xmlDeserializer = xmlDeserializer;
            return this;
        }

        public string Serialize(BodyParameter bodyParameter)
        {
            var savedNamespace = _xmlSerializer.Namespace;
            _xmlSerializer.Namespace = bodyParameter.XmlNamespace ?? savedNamespace;

            var result = _xmlSerializer.Serialize(bodyParameter.Value);

            _xmlSerializer.Namespace = savedNamespace;

            return result;
        }

        private XmlSerilizationOptions _options = XmlSerilizationOptions.Default;
        private IXmlSerializer _xmlSerializer = new XmlSerializer();
        private IXmlDeserializer _xmlDeserializer = new XmlDeserializer();

        public string RootElement
        {
            get => _options.RootElement;
            set => _options.RootElement = value;
        }

        public string Namespace
        {
            get => _options.Namespace;
            set => _options.Namespace = value;
        }

        public string DateFormat
        {
            get => _options.DateFormat;
            set => _options.DateFormat = value;
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

        public static XmlSerilizationOptions Default =>
            new XmlSerilizationOptions
            {
                Culture = CultureInfo.InvariantCulture
            };
    }
}