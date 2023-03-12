using CsvHelper.Configuration;

namespace RestSharp.Serializers.CsvHelper; 

[PublicAPI]
public static class RestClientExtensions {
    public static SerializerConfig UseCsvHelper(this SerializerConfig config) => config.UseSerializer<CsvHelperSerializer>();

    public static SerializerConfig UseCsvHelper(this SerializerConfig config, CsvConfiguration configuration)
        => config.UseSerializer(() => new CsvHelperSerializer(configuration));
}
