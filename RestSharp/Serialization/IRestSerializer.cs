using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization
{
    public interface IRestSerializer : ISerializer, IDeserializer
    {
        string ContentType { get; }
        
        string[] SupportedContentTypes { get; }
        
        DataFormat DataFormat { get; }

        string Serialize(BodyParameter bodyParameter);
    }
}