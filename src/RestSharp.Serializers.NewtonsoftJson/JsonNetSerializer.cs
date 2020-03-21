using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp.Serialization;

namespace RestSharp.Serializers.NewtonsoftJson
{
    public class JsonNetSerializer : IRestSerializer
    {
        public bool UseBytes { get; } = false;

        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver     = new CamelCasePropertyNamesContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling     = TypeNameHandling.None,
            NullValueHandling    = NullValueHandling.Ignore,
            Formatting           = Formatting.None,
            ConstructorHandling  = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

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

        public string Serialize(object obj)
        {
            using var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);

            using var jsonTextWriter = new JsonTextWriter(stringWriter)
            {
                Formatting = _serializer.Formatting, CloseOutput = true
            };

            _serializer.Serialize(jsonTextWriter, obj, obj.GetType());
            
            return stringWriter.ToString();
        }

        public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

        public byte[] SerializeToBytes(object obj)
            => throw new NotSupportedException("Serialize obj to JsonNet byte[] array is not supported!");


        public T Deserialize<T>(IRestResponse response)
            => this.Deserialize<T>(response.Content);

        public T Deserialize<T>(string payload)
        {
            using var reader = new JsonTextReader(new StringReader(payload)) {CloseInput = true};

            return _serializer.Deserialize<T>(reader);
        }

        public T DeserializeFromBytes<T>(byte[] payload)
            => throw new NotSupportedException("Deserialize JsonNet from byte[] array is not supported!");


        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}