using System.IO;
using System.IO.Compression;

namespace RestSharp.Serializers.Compression.GZIP
{
    public class GZipCompressor : ICompressor
    {
         
        public byte[] Compress(byte[] payload)
        {
            var decompressedStream = new MemoryStream(payload);
            var compressedStream = new MemoryStream();

            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                decompressedStream.CopyTo(gzipStream);
            }

            return compressedStream.ToArray();
        }


        public byte[] Decompress(byte[] payload)
        {
            var compressedStream = new MemoryStream(payload);
            var decompressedStream = new MemoryStream();

            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedStream);
            }

            return decompressedStream.ToArray();
        }


    }
}
