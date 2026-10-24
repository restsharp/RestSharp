using System.IO.Compression;
using RestSharp.Extensions;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Integrated;

public class CompressionTests {
    static async Task<byte[]> GetBody(Func<Stream, Stream> getStream, string value) {
        using var memoryStream = new MemoryStream();

        // ReSharper disable once UseAwaitUsing
        using (var stream = getStream(memoryStream)) {
            stream.WriteStringUtf8(value);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        var body = await memoryStream.ReadAsBytes(default);
        return body;
    }

    static void ConfigureServer(WireMockServer server, byte[] body, string encoding)
        => server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody(body).WithHeader("Content-Encoding", encoding));

    [Fact]
    public async Task Can_Handle_Deflate_Compressed_Content() {
        const string value  = "This is some deflated content";
        using var    server = WireMockServer.Start();

        var body = await GetBody(s => new DeflateStream(s, CompressionMode.Compress, true), value);
        ConfigureServer(server, body, "deflate");

        using var client   = new RestClient(server.Url!, options => options.AutomaticDecompression = DecompressionMethods.Deflate);
        var       request  = new RestRequest("");
        var       response = await client.ExecuteAsync(request);

        response.Content.Should().Be(value);
    }

    [Fact]
    public async Task Can_Handle_Gzip_Compressed_Content() {
        const string value  = "This is some gzipped content";
        using var    server = WireMockServer.Start();

        var body = await GetBody(s => new GZipStream(s, CompressionMode.Compress, true), value);
        ConfigureServer(server, body, "gzip");

        using var client   = new RestClient(server.Url!);
        var       request  = new RestRequest("");
        var       response = await client.ExecuteAsync(request);

        response.Content.Should().Be(value);
    }

    [Fact]
    public async Task Can_Handle_Uncompressed_Content() {
        const string value  = "This is some sample content";
        using var    server = WireMockServer.Start();

        server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody(value));

        using var client   = new RestClient(server.Url!);
        var       request  = new RestRequest("");
        var       response = await client.ExecuteAsync(request);

        response.Content.Should().Be(value);
    }

#if NET8_0_OR_GREATER
    // Brotli is excluded from the default AutomaticDecompression because the Brotli decoder
    // bundled with the .NET runtime is affected by CVE-2026-111497. See docs/advanced/configuration.md.
    [Fact]
    public async Task Default_Client_Does_Not_Advertise_Or_Decode_Brotli() {
        const string value  = "This is some brotli content";
        using var    server = WireMockServer.Start();

        var body = await GetBody(s => new BrotliStream(s, CompressionMode.Compress, true), value);
        ConfigureServer(server, body, "br");

        using var client   = new RestClient(server.Url!);
        var       request  = new RestRequest("");
        var       response = await client.ExecuteAsync(request);

        var acceptEncoding = server.LogEntries.Single().RequestMessage.Headers!["Accept-Encoding"].ToString();
        acceptEncoding.Should().NotContain("br");

        // The body is returned as-is: the Brotli decoder is never invoked
        response.RawBytes.Should().Equal(body);
        response.Content.Should().NotBe(value);
    }

    [Fact]
    public async Task Can_Handle_Brotli_Compressed_Content_When_Opted_In() {
        const string value  = "This is some brotli content";
        using var    server = WireMockServer.Start();

        var body = await GetBody(s => new BrotliStream(s, CompressionMode.Compress, true), value);
        ConfigureServer(server, body, "br");

        using var client   = new RestClient(server.Url!, options => options.AutomaticDecompression = DecompressionMethods.All);
        var       request  = new RestRequest("");
        var       response = await client.ExecuteAsync(request);

        response.Content.Should().Be(value);
    }
#endif
}