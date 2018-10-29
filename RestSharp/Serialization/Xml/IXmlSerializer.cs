using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public interface IXmlSerializer : ISerializer
    {
        string RootElement { get; set; }

        string Namespace { get; set; }

        string DateFormat { get; set; }
    }
}
