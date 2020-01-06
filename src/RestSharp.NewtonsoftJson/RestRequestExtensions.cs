using Newtonsoft.Json;

namespace RestSharp.NewtonsoftJson
{
    public static class RestRequestExtensions
    {
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request)
        {
            request.JsonSerializer = new JsonNetSerializer();
            return request;
        }
        
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request, JsonSerializerSettings settings)
        {
            request.JsonSerializer = new JsonNetSerializer(settings);
            return request;
        }
    }
}