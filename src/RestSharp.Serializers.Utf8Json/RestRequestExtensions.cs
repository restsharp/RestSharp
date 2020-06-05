using Utf8Json;

namespace RestSharp.Serializers.Utf8Json
{
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Use Utf8Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IRestRequest UseUtf8Json(this IRestRequest request)
        {
            request.JsonSerializer = new Utf8JsonSerializer();
            return request;
        }
        
        /// <summary>
        /// Use Utf8Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="resolver">JSON formatter resolver instance to provide custom options to Utf8Json</param>
        /// <returns></returns>
        public static IRestRequest UseUtf8Json(this IRestRequest request, IJsonFormatterResolver resolver)
        {
            request.JsonSerializer = new Utf8JsonSerializer(resolver);
            return request;
        }
    }
}