using System;

namespace RestSharp.Extensions;

/// <summary>
/// Extension method overload!
/// </summary>
static class MiscExtensions {
    /// <summary>
    /// Read a stream into a byte array
    /// </summary>
    /// <param name="input">Stream to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns>byte[]</returns>
    public static async Task<byte[]> ReadAsBytes(this Stream input, CancellationToken cancellationToken) {
        var buffer = new byte[16 * 1024];

        using var ms = new MemoryStream();

        int read;
#if NETSTANDARD
        while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
#else
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
#endif
            ms.Write(buffer, 0, read);

        return ms.ToArray();
    }
}