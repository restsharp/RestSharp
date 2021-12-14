using System.Text.Json;

namespace RestSharp.Serializers.Json; 

public class SystemTextJsonSerializer : IRestSerializer {
    readonly JsonSerializerOptions _options;

    /// <summary>
    /// Create the new serializer that uses System.Text.Json.JsonSerializer with default settings
    /// </summary>
    public SystemTextJsonSerializer() => _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Create the new serializer that uses System.Text.Json.JsonSerializer with custom settings
    /// </summary>
    /// <param name="options">Json serializer settings</param>
    public SystemTextJsonSerializer(JsonSerializerOptions options) => _options = options;

    public string? Serialize(object? obj) => obj == null ? null : JsonSerializer.Serialize(obj, _options);

    public string? Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T? Deserialize<T>(RestResponse response) => JsonSerializer.Deserialize<T>(response.Content!, _options);

    public string[] SupportedContentTypes { get; } = {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat => DataFormat.Json;
}