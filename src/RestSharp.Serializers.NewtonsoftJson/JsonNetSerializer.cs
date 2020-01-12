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

        public T Deserialize<T>(IRestResponse response)
        {
            using var reader = new JsonTextReader(new StringReader(response.Content)) {CloseInput = true};

            return _serializer.Deserialize<T>(reader);
        }

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}