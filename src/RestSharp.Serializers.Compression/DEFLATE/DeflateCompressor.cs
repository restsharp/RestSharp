using System.IO;
using System.IO.Compression;

namespace RestSharp.Serializers.Compression.DEFLATE
{
    public class DeflateCompressor : ICompressor
    {
         
        public byte[] Compress(byte[] payload)
        {
            var decompressedStream = new MemoryStream(payload);
            var compressedStream = new MemoryStream();

            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress))
            {
                decompressedStream.CopyTo(deflateStream);
            }

            return compressedStream.ToArray();
        }


        public byte[] Decompress(byte[] payload)
        {
            var compressedStream = new MemoryStream(payload);
            var decompressedStream = new MemoryStream();

            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(decompressedStream);
            }

            return decompressedStream.ToArray();
        }


    }
}
