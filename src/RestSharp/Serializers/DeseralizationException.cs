// ReSharper disable once CheckNamespace
namespace RestSharp;

public class DeserializationException : Exception {
    public DeserializationException(RestResponse response, Exception innerException)
        : base("Error occured while deserializing the response", innerException)
        => Response = response;

    public RestResponse Response { get; }
}