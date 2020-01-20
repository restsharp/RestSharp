namespace RestSharp.Serializers.MessagePack
{
    public static class RestRequestExtensions
    {
        public static IRestRequest AddMessagePackBody(this IRestRequest request, object obj, global::MessagePack.MessagePackSerializerOptions options = null)
        {
            request.RequestFormat = DataFormat.None;

            var bytes = global::MessagePack.MessagePackSerializer.Serialize(obj, options);
            var parameter = new Parameter("", bytes, "application/x-msgpack", ParameterType.RequestBody);
            return request.AddParameter(parameter);
        }
    }
}
