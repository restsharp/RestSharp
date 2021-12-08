namespace RestSharp.Serializers; 

public interface IDeserializer {
    T? Deserialize<T>(IRestResponse response);
}