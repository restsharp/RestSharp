using System;
using System.IO;

namespace RestSharp.Serializers.Compression
{
    public interface ICompressor 
    {

        byte[] Compress(byte[] payload);


        byte[] Decompress(byte[] payload);

    }
}
