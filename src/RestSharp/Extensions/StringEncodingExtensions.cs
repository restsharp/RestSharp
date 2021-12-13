using System.Text;

namespace RestSharp.Extensions;

public static class StringEncodingExtensions {
    /// <summary>
    /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
    /// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
    /// </summary>
    /// <param name="buffer">An array of bytes to convert</param>
    /// <param name="encoding">Content encoding. Will fallback to UTF8 if not a valid encoding.</param>
    /// <returns>The byte as a string.</returns>
    [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
    public static string AsString(this byte[] buffer, string? encoding) {
        var enc = encoding.IsEmpty() ? Encoding.UTF8 : TryParseEncoding();

        return AsString(buffer, enc);

        Encoding TryParseEncoding() {
            try {
                return encoding != null ? Encoding.GetEncoding(encoding) : Encoding.UTF8;
            }
            catch (ArgumentException) {
                return Encoding.UTF8;
            }
        }
    }

    static string AsString(byte[]? buffer, Encoding encoding) => buffer == null ? "" : encoding.GetString(buffer, 0, buffer.Length);
}