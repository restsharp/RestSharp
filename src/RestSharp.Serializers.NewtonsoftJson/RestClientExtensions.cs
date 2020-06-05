using Newtonsoft.Json;

namespace RestSharp.Serializers.NewtonsoftJson
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use Newtonsoft.Json serializer with default settings
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IRestClient UseNewtonsoftJson(this IRestClient client) => client.UseSerializer(() => new JsonNetSerializer());

        /// <summary>
        /// Use Newtonsoft.Json serializer with custom settings
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings">Newtonsoft.Json serializer settings</param>
        /// <returns></returns>
        public static IRestClient UseNewtonsoftJson(this IRestClient client, JsonSerializerSettings settings)
            => client.UseSerializer(() => new JsonNetSerializer(settings));
    }
}