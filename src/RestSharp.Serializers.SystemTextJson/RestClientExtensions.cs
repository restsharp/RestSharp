using System.Text.Json;

namespace RestSharp.Serializers.SystemTextJson
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use System.Text.Json serializer with default settings
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IRestClient UseSystemTextJson(this IRestClient client) => client.UseSerializer(() => new SystemTextJsonSerializer());

        /// <summary>
        /// Use System.Text.Json serializer with custom settings
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options">System.Text.Json serializer options</param>
        /// <returns></returns>
        public static IRestClient UseSystemTextJson(this IRestClient client, JsonSerializerOptions options)
            => client.UseSerializer(() => new SystemTextJsonSerializer(options));
    }
}