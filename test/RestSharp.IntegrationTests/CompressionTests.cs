using System;
using System.IO.Compression;
using System.Net;
using NUnit.Framework;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class CompressionTests
    {
        static Action<HttpListenerContext> GzipEchoValue(string value)
            => context =>
            {
                context.Response.Headers.Add("Content-encoding", "gzip");

                using var gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true);
                
                gzip.WriteStringUtf8(value);
            };

        static Action<HttpListenerContext> DeflateEchoValue(string value)
            => context =>
            {
                context.Response.Headers.Add("Content-encoding", "deflate");

                using var gzip = new DeflateStream(context.Response.OutputStream, CompressionMode.Compress, true);

                gzip.WriteStringUtf8(value);
            };

        [Test]
        public void Can_Handle_Deflate_Compressed_Content()
        {
            using var server = SimpleServer.Create(DeflateEchoValue("This is some deflated content"));

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("");
            var response = client.Execute(request);

            Assert.AreEqual("This is some deflated content", response.Content);
        }

        [Test]
        public void Can_Handle_Gzip_Compressed_Content()
        {
            using var server = SimpleServer.Create(GzipEchoValue("This is some gzipped content"));

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("");
            var response = client.Execute(request);

            Assert.AreEqual("This is some gzipped content", response.Content);
        }

        [Test]
        public void Can_Handle_Uncompressed_Content()
        {
            using var server = SimpleServer.Create(Handlers.EchoValue("This is some sample content"));

            var client   = new RestClient(server.Url);
            var request  = new RestRequest("");
            var response = client.Execute(request);

            Assert.AreEqual("This is some sample content", response.Content);
        }
    }
}