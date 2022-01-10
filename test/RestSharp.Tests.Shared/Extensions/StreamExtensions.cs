using System.Text;

namespace RestSharp.Tests.Shared.Extensions;

public static class StreamExtensions {
    public static void WriteStringUtf8(this Stream target, string value) {
        var encoded = Encoding.UTF8.GetBytes(value);

        target.Write(encoded, 0, encoded.Length);
    }

    public static string StreamToString(this Stream stream) {
        using var streamReader = new StreamReader(stream);

        return streamReader.ReadToEnd();
    }

    public static async Task<string> StreamToStringAsync(this Stream stream) {
        using var streamReader = new StreamReader(stream);

        return await streamReader.ReadToEndAsync().ConfigureAwait(false);
    }
}