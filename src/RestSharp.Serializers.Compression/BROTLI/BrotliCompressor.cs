using Brotli;
using System.IO;
using System.IO.Compression;


namespace RestSharp.Serializers.Compression.BROTLI
{
    public class BrotliCompressor : ICompressor
    {
         
        public byte[] Compress(byte[] payload)
        {
            using (System.IO.MemoryStream msInput = new System.IO.MemoryStream(payload))
            using (System.IO.MemoryStream msOutput = new System.IO.MemoryStream())
            using (BrotliStream bs = new BrotliStream(msOutput, System.IO.Compression.CompressionMode.Compress))
            {
                bs.SetQuality(11);
                bs.SetWindow(22);
                msInput.CopyTo(bs);
                bs.Close();
                return msOutput.ToArray();
            }
        }


        public byte[] Decompress(byte[] payload)
        {
            using (System.IO.MemoryStream msInput = new System.IO.MemoryStream(payload))
            using (BrotliStream bs = new BrotliStream(msInput, System.IO.Compression.CompressionMode.Decompress))
            using (System.IO.MemoryStream msOutput = new System.IO.MemoryStream())
            {
                bs.CopyTo(msOutput);
                msOutput.Seek(0, System.IO.SeekOrigin.Begin);
                return msOutput.ToArray();
            }
        }


    }
}
