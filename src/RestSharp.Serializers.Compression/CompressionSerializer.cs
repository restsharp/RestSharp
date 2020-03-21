using RestSharp.Serialization;
using RestSharp.Serializers.Compression.GZIP;
using System;
using System.Text;

namespace RestSharp.Serializers.Compression
{
    public class CompressionSerializer : IRestSerializer
    {

        public bool UseBytes { get; } = true;

        ICompressor Compressor { get; }

        IRestSerializer ChainedSerializer { get; }


        public CompressionSerializer(
                    IRestSerializer chainedSerializer = null,
                    ICompressor compressor = null)
        {
            Compressor = compressor ?? new GZipCompressor();
            ChainedSerializer = chainedSerializer;
        }



        public string Serialize(object obj)
        {
            throw new NotSupportedException("Serialize obj to compressed string has no use!");
        }

        public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);


        public byte[] SerializeToBytes(object obj)
        {
            object serializedObj = obj;
            if (ChainedSerializer != null)
            {
                serializedObj = ChainedSerializer.Serialize(obj);
            }

            byte[] toCompress = null;
            if ( serializedObj.GetType().Equals(typeof(string)))
            {
                toCompress = Encoding.UTF8.GetBytes((string)serializedObj);
            }

 
            return Compressor.Compress(toCompress);
        }




        public T Deserialize<T>(IRestResponse response)
        {
            return this.DeserializeFromBytes<T>(response.RawBytes);
        }


        public T Deserialize<T>(string payload)
            => throw new NotSupportedException("Deserializing/Decompressing a string has no use so not supported!");


        public T DeserializeFromBytes<T>(byte[] payload)
        {
            T resultObject = default;

            byte[] decompPayload = Compressor.Decompress(payload);

            string strObj = Encoding.UTF8.GetString(decompPayload);

            if (!String.IsNullOrEmpty(strObj))
            {

                if (ChainedSerializer != null)
                {
                    resultObject = ChainedSerializer.Deserialize<T>(strObj);
                }
                else
                {
                    resultObject = default;
                }
            }

            return resultObject;
        }




        public string[] SupportedContentTypes { get; } =
        {
            "application/x-gzip"
        };

        public string ContentType { get; set; } = "application/x-gzip";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}