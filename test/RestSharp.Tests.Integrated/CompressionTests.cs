using System.IO.Compression;
using System.Net;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated; 

public class CompressionTests {
    readonly ITestOutputHelper _output;

    static Action<HttpListenerContext> GzipEchoValue(string value)
        => context => {
            context.Response.Headers.Add("Content-encoding", "gzip");

            using var gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true);

            gzip.WriteStringUtf8(value);
        };

    static Action<HttpListenerContext> DeflateEchoValue(string value)
        => context => {
            context.Response.Headers.Add("Content-encoding", "deflate");

            using var gzip = new DeflateStream(context.Response.OutputStream, CompressionMode.Compress, true);

            gzip.WriteStringUtf8(value);
        };
    
    public CompressionTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Can_Handle_Deflate_Compressed_Content() {
        using var server = SimpleServer.Create(DeflateEchoValue("This is some deflated content"));

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("");
        var response = await client.ExecuteAsync(request);

        Assert.Equal("This is some deflated content", response.Content);
    }

    [Fact]
    public async Task Can_Handle_Gzip_Compressed_Content() {
        using var server = SimpleServer.Create(GzipEchoValue("This is some gzipped content"));

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("");
        var response = await client.ExecuteAsync(request);

        Assert.Equal("This is some gzipped content", response.Content);
    }

    [Fact]
    public async Task Can_Handle_Uncompressed_Content() {
        using var server = SimpleServer.Create(Handlers.EchoValue("This is some sample content"));

        var client   = new RestClient(server.Url);
        var request  = new RestRequest("");
        var response = await client.ExecuteAsync(request);

        Assert.Equal("This is some sample content", response.Content);
    }
}