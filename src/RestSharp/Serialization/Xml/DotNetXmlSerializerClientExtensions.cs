using System.Text;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public static class DotNetXmlSerializerClientExtensions
    {
        public static IRestClient UseDotNetXmlSerializer(
            this IRestClient restClient,
            string xmlNamespace = null,
            Encoding encoding = null
        )
        {
            var xmlSerializer                                 = new DotNetXmlSerializer();
            if (xmlNamespace != null) xmlSerializer.Namespace = xmlNamespace;
            if (encoding     != null) xmlSerializer.Encoding  = encoding;

            var xmlDeserializer                            = new DotNetXmlDeserializer();
            if (encoding != null) xmlDeserializer.Encoding = encoding;

            var serializer = new XmlRestSerializer()
                .WithXmlSerializer(xmlSerializer)
                .WithXmlDeserializer(xmlDeserializer);

            return restClient.UseSerializer(() => serializer);
        }
    }
}