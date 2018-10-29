using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization
{
    public interface IRestSerializer : ISerializer, IDeserializer
    {
    }
}