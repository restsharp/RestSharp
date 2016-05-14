#if DNXCORE50
using System;
using System.IO;

namespace RestSharp.Extensions
{
    public static class MemoryStreamExtensions
    {
        public static byte[] GetBuffer(this MemoryStream stream)
        {
            ArraySegment<byte> buffer;
            if (stream.TryGetBuffer(out buffer))
            {
                return buffer.Array;
            }

            return new byte[0];
        }
    }
}
#endif