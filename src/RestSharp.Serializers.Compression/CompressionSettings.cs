using RestSharp.Serializers.Compression.GZIP;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Serializers.Compression
{

    /*
    public class CompressionSettings
    {
        // Default GZIP support
        public string CompressionType { get; set; } = CompressionTypes.GZIP;


        public ICompressor Compressor = new GZIPCompressor()

    }
    */

    public static class CompressionTypes 
    {
        public const string GZIP = "GZIP";
        
        public const string DEFLATE = "DEFLATE";

        public const string BROTLI = "BROTLI";
    }
}
