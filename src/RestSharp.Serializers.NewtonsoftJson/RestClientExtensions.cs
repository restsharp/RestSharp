using Newtonsoft.Json;

namespace RestSharp.Serializers.NewtonsoftJson
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use Json.Net serializer with default settings
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IRestClient UseNewtonsoftJson(this IRestClient client) => client.UseSerializer(() => new JsonNetSerializer());

        /// <summary>
        /// Use Json.Net serializer with custom settings
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings">Json.Net serializer settings</param>
        /// <returns></returns>
        public static IRestClient UseNewtonsoftJson(this IRestClient client, JsonSerializerSettings settings)
            => client.UseSerializer(() => new JsonNetSerializer(settings));
    }
}