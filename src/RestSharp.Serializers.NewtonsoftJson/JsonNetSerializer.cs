using Newtonsoft.Json.Serialization;

namespace RestSharp.Serializers.NewtonsoftJson; 

public class JsonNetSerializer : IRestSerializer {
    /// <summary>
    /// Default serialization settings:
    /// - Camel-case contract resolver
    /// - Type name handling set to none
    /// - Null values ignored
    /// - Non-indented formatting
    /// - Allow using non-public constructors
    /// </summary>
    public static readonly JsonSerializerSettings DefaultSettings = new() {
        ContractResolver     = new CamelCasePropertyNamesContractResolver(),
        DefaultValueHandling = DefaultValueHandling.Include,
        TypeNameHandling     = TypeNameHandling.None,
        NullValueHandling    = NullValueHandling.Ignore,
        Formatting           = Formatting.None,
        ConstructorHandling  = ConstructorHandling.AllowNonPublicDefaultConstructor
    };

    [ThreadStatic] static WriterBuffer? _writerBuffer;

    readonly JsonSerializer _serializer;

    /// <summary>
    /// Create the new serializer that uses Json.Net with default settings
    /// </summary>
    public JsonNetSerializer() => _serializer = JsonSerializer.Create(DefaultSettings);

    /// <summary>
    /// Create the new serializer that uses Json.Net with custom settings
    /// </summary>
    /// <param name="settings">Json.Net serializer settings</param>
    public JsonNetSerializer(JsonSerializerSettings settings) => _serializer = JsonSerializer.Create(settings);

    public string? Serialize(object? obj) {
        if (obj == null) return null;
            
        using var writerBuffer = _writerBuffer ??= new WriterBuffer(_serializer);

        _serializer.Serialize(writerBuffer.GetJsonTextWriter(), obj, obj.GetType());

        return writerBuffer.GetStringWriter().ToString();
    }

    public string? Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T? Deserialize<T>(RestResponse response) {
        if (response.Content == null)
            throw new DeserializationException(response, new InvalidOperationException("Response content is null"));
        using var reader = new JsonTextReader(new StringReader(response.Content)) { CloseInput = true };

        return _serializer.Deserialize<T>(reader);
    }

    public string[] SupportedContentTypes { get; } = {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat => DataFormat.Json;
}