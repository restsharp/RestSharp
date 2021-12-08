using System.Text;

namespace RestSharp.Extensions;

static class StreamExtensions {
    public static void WriteString(this Stream stream, string value, Encoding encoding) {
        var bytes = encoding.GetBytes(value);

        stream.Write(bytes, 0, bytes.Length);
    }

    public static Task WriteStringAsync(this Stream stream, string value, Encoding encoding, CancellationToken cancellationToken) {
        var bytes = encoding.GetBytes(value);

        return stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
    }
}