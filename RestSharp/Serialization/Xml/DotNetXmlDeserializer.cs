using System.IO;
using System.Text;
using RestSharp.Serialization.Xml;

namespace RestSharp.Deserializers
{
    /// <summary>
    /// Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlDeserializer : IXmlDeserializer
    {
        /// <summary>
        ///     Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        ///     XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        /// <summary>
        ///     Encoding for serialized content
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        
        public T Deserialize<T>(IRestResponse response)
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                return default(T);
            }

            using (var stream = new MemoryStream(Encoding.GetBytes(response.Content)))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

                return (T) serializer.Deserialize(stream);
            }
        }
    }
}