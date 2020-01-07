using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public interface IXmlSerializer : ISerializer, IWithRootElement
    {
        string Namespace { get; set; }

        string DateFormat { get; set; }
    }
}