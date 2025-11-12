using CsvHelper.Configuration;

namespace RestSharp.Serializers.CsvHelper; 

[PublicAPI]
public static class RestClientExtensions {
    extension(SerializerConfig config) {
        public SerializerConfig UseCsvHelper() => config.UseSerializer<CsvHelperSerializer>();

        public SerializerConfig UseCsvHelper(CsvConfiguration configuration)
            => config.UseSerializer(() => new CsvHelperSerializer(configuration));
    }
}
