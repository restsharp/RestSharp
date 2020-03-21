using Newtonsoft.Json;

namespace RestSharp.Serializers.Compression
{
    public static class RestRequestExtensions
    {
        public static IRestRequest UseCompressedNewtonsoftJson(this IRestRequest request)
        {
            request.JsonSerializer = new CompressionSerializer();
            return request;
        }
        




    }
}