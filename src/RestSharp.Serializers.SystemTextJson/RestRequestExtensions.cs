using System.Text.Json;

namespace RestSharp.Serializers.SystemTextJson
{
    public static class RestRequestExtensions
    {
        public static IRestRequest UseSystemTextJson(this IRestRequest request)
        {
            request.JsonSerializer = new SystemTextJsonSerializer();
            return request;
        }
        
        public static IRestRequest UseSystemTextJson(this IRestRequest request, JsonSerializerOptions options)
        {
            request.JsonSerializer = new SystemTextJsonSerializer(options);
            return request;
        }
    }
}