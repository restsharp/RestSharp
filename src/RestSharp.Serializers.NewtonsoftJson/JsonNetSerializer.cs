//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using Newtonsoft.Json.Serialization;

namespace RestSharp.Serializers.NewtonsoftJson;

public class JsonNetSerializer : IRestSerializer, ISerializer, IDeserializer {
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

        using var buffer = _writerBuffer ??= new(_serializer);

        _serializer.Serialize(buffer.GetJsonTextWriter(), obj, obj.GetType());

        return buffer.GetStringWriter().ToString();
    }

    public string? Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T? Deserialize<T>(RestResponse response) {
        if (response.Content == null)
            throw new DeserializationException(response, new InvalidOperationException("Response content is null"));

        using var reader = new JsonTextReader(new StringReader(response.Content)) { CloseInput = true };

        return _serializer.Deserialize<T>(reader);
    }

    public ISerializer   Serializer   => this;
    public IDeserializer Deserializer => this;

    public string[] AcceptedContentTypes => ContentType.JsonAccept;

    public ContentType ContentType { get; set; } = ContentType.Json;

    public SupportsContentType SupportsContentType => contentType => contentType.Value.Contains("json");

    public DataFormat DataFormat => DataFormat.Json;
}