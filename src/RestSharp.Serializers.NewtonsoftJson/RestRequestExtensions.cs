using Newtonsoft.Json;

namespace RestSharp.Serializers.NewtonsoftJson
{
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Use Newtonsoft.Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request)
        {
            request.JsonSerializer = new JsonNetSerializer();
            return request;
        }
        
        /// <summary>
        /// Use Newtonsoft.Json serializer for a single request, with custom settings
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings">Newtonsoft.Json serializer settings</param>
        /// <returns></returns>
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request, JsonSerializerSettings settings)
        {
            request.JsonSerializer = new JsonNetSerializer(settings);
            return request;
        }
    }
}