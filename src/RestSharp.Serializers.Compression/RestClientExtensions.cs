using RestSharp.Serialization;

namespace RestSharp.Serializers.Compression
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use Json.Net serializer with default settings
        /// </summary>
        /// <param name="client"></param>
        /// <param name="chainedSerializer"></param>
        /// <param name="compressor"></param>
        /// <returns></returns>
        public static IRestClient UseCompression(this IRestClient client, IRestSerializer chainedSerializer, ICompressor compressor = null ) => client.UseSerializer(() => new CompressionSerializer(chainedSerializer, compressor));

    }
}