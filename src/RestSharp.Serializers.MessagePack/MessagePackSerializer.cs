using System;
using RestSharp.Serialization;

namespace RestSharp.Serializers.MessagePack
{
    public class MessagePackSerializer : IRestSerializer
    {
        private readonly global::MessagePack.MessagePackSerializerOptions _options;

        public MessagePackSerializer()
        {
        }

        public MessagePackSerializer(global::MessagePack.MessagePackSerializerOptions options) => _options = options;

        // This method is not called when DataFormat is None.
        public string Serialize(object obj) => throw new NotSupportedException("MessagePack is a binary serialization format.");
        
        public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

        public T Deserialize<T>(IRestResponse response) =>
            global::MessagePack.MessagePackSerializer.Deserialize<T>(response.RawBytes, _options);

        public string[] SupportedContentTypes { get; } =
        {
            "application/x-msgpack"
        };

        public string ContentType { get; set; } = "application/x-msgpack";

        public DataFormat DataFormat { get; } = DataFormat.None;
    }
}
