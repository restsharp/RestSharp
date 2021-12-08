namespace RestSharp.Extensions;

/// <summary>
/// Extension method overload!
/// </summary>
public static class MiscExtensions {
    /// <summary>
    /// Read a stream into a byte array
    /// </summary>
    /// <param name="input">Stream to read</param>
    /// <returns>byte[]</returns>
    [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
    public static byte[] ReadAsBytes(this Stream input) {
        var buffer = new byte[16 * 1024];

        using var ms = new MemoryStream();

        int read;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            ms.Write(buffer, 0, read);

        return ms.ToArray();
    }
}