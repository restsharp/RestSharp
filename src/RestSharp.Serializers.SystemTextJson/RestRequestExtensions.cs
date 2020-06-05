using System.Text.Json;

namespace RestSharp.Serializers.SystemTextJson
{
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Use System.Text.Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IRestRequest UseSystemTextJson(this IRestRequest request)
        {
            request.JsonSerializer = new SystemTextJsonSerializer();
            return request;
        }
        
        /// <summary>
        /// Use System.Text.Json serializer for a single request with custom options
        /// </summary>
        /// <param name="request"></param>
        /// <param name="options">System.Text.Json serializer options</param>
        /// <returns></returns>
        public static IRestRequest UseSystemTextJson(this IRestRequest request, JsonSerializerOptions options)
        {
            request.JsonSerializer = new SystemTextJsonSerializer(options);
            return request;
        }
    }
}