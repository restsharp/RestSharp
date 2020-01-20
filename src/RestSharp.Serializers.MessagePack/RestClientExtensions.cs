namespace RestSharp.Serializers.MessagePack
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use "MessagePack for C#" deserializer
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IRestClient UseMessagePack(this IRestClient client, global::MessagePack.MessagePackSerializerOptions options = null)
            => client.UseSerializer(() => new MessagePackSerializer(options));
    }
}
