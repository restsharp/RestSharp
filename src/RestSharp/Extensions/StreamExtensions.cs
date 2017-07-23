using System.IO;

namespace RestSharp.Extensions
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Copies bytes from one stream to another
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        public static void CopyTo(this Stream input, Stream output)
        {
#if NET35
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
#else
            input.CopyTo(output);
#endif
        }
    }
}
