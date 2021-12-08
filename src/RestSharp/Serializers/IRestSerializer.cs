namespace RestSharp.Serializers; 

public interface IRestSerializer : ISerializer, IDeserializer {
    string[] SupportedContentTypes { get; }

    DataFormat DataFormat { get; }

    string? Serialize(Parameter parameter);
}