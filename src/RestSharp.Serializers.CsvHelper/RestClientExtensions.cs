using CsvHelper.Configuration;

namespace RestSharp.Serializers.CsvHelper {
    [PublicAPI]
    public static class RestClientExtensions {
        public static RestClient UseCsvHelper(this RestClient client) {
            return client.UseSerializer<CsvHelperSerializer>();
        }

        public static RestClient UseCsvHelper(this RestClient client, CsvConfiguration configuration) {
            return client.UseSerializer(() => new CsvHelperSerializer(configuration));
        }
    }
}