using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization
{
    public interface IRestSerializer : ISerializer, IDeserializer
    {
        string[] SupportedContentTypes { get; }

        DataFormat DataFormat { get; }

        string Serialize(Parameter parameter);
    }
}