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

        using var client   = new RestClient(server.Url!);
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
}