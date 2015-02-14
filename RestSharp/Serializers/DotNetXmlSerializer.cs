using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RestSharp.Serializers
{
    /// <summary>
    /// Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlSerializer : ISerializer
    {
        /// <summary>
        /// Default constructor, does not specify namespace
        /// </summary>
        public DotNetXmlSerializer()
        {
            ContentType = "application/xml";
            Settings = new XmlWriterSettings();
#pragma warning disable 0618
            Settings.Encoding = Encoding.UTF8;
#pragma warning restore 0618
        }

        /// <summary>
        /// Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public DotNetXmlSerializer(string @namespace)
            : this()
        {
            Namespace = @namespace;
        }

        /// <summary>
        /// Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML as string</returns>
        public string Serialize(object obj)
        {
            var ns = new XmlSerializerNamespaces();

            ns.Add(string.Empty, Namespace);

            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var writer = new StringWriter();

#pragma warning disable 0618
            if (Encoding != Settings.Encoding
                && Encoding != Encoding.UTF8)
            {
                Settings.Encoding = Encoding;
            }
#pragma warning restore 0618

            var xmlWriter = XmlWriter.Create(writer, Settings);

            serializer.Serialize(xmlWriter, obj, ns);

            return writer.ToString();
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

        /// <summary>
        /// Encoding for serialized content
        /// </summary>
        [ObsoleteAttribute("You should use Settings.Encoding instead.", false)]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Settings for serialized content
        /// </summary>
        public XmlWriterSettings Settings { get; set; }

    }
}
