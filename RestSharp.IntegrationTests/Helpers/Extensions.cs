using System.IO;
using System.Text;

namespace RestSharp.IntegrationTests.Helpers
{
    public static class Extensions
    {
        public static void WriteStringUtf8(this Stream target, string value)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(value);

            target.Write(encoded, 0, encoded.Length);
        }
    }
}
