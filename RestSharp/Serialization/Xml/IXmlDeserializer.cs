using RestSharp.Deserializers;

namespace RestSharp.Serialization.Xml
{
    public interface IXmlDeserializer : IDeserializer, IWithRootElement
    {
        string Namespace { get; set; }

        string DateFormat { get; set; }
    }
}