using JetBrains.Annotations;
using Utf8Json;

namespace RestSharp.Serializers.Utf8Json {
    [PublicAPI]
    public static class RestClientExtensions {
        /// <summary>
        /// Use Utf8Json serializer with default formatter resolver
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IRestClient UseUtf8Json(this IRestClient client) => client.UseSerializer(() => new Utf8JsonSerializer());

        /// <summary>
        /// Use Utf8Json serializer with custom formatter resolver
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resolver">Utf8Json deserialization formatter resolver</param>
        /// <returns></returns>
        public static IRestClient UseUtf8Json(this IRestClient client, IJsonFormatterResolver resolver)
            => client.UseSerializer(() => new Utf8JsonSerializer(resolver));
    }
}