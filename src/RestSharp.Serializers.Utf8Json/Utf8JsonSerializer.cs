using System.Text;
using RestSharp.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RestSharp.Serializers.Utf8Json
{
    public class Utf8JsonSerializer : IRestSerializer
    {
        public Utf8JsonSerializer(IJsonFormatterResolver resolver = null) => Resolver = resolver ?? StandardResolver.AllowPrivateExcludeNullCamelCase;

        IJsonFormatterResolver Resolver { get; }

        public string Serialize(object obj) => JsonSerializer.NonGeneric.ToJsonString(obj, Resolver);

        public string Serialize(Parameter parameter) => Serialize(parameter.Value);

        public T Deserialize<T>(IRestResponse response) => JsonSerializer.Deserialize<T>(response.Content, Resolver);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}