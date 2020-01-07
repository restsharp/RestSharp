using Utf8Json;

namespace RestSharp.Serializers.Utf8Json
{
    public static class RestRequestExtensions
    {
        public static IRestRequest UseUtf8Json(this IRestRequest request)
        {
            request.JsonSerializer = new Utf8JsonSerializer();
            return request;
        }
        
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request, IJsonFormatterResolver resolver)
        {
            request.JsonSerializer = new Utf8JsonSerializer(resolver);
            return request;
        }
    }
}