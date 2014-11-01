using System;
using System.IO.Compression;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    public class CompressionTests
    {
        [Fact]
        public void Can_Handle_Gzip_Compressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using(SimpleServer.Create(baseUrl.AbsoluteUri, GzipEchoValue("This is some gzipped content")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("");
                var response = client.Execute(request);

                Assert.Equal("This is some gzipped content", response.Content);
            }
        }

        [Fact]
        public void Can_Handle_Deflate_Compressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using(SimpleServer.Create(baseUrl.AbsoluteUri, DeflateEchoValue("This is some deflated content")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("");
                var response = client.Execute(request);

                Assert.Equal("This is some deflated content", response.Content);
            }
        }

        [Fact]
        public void Can_Handle_Uncompressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using(SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue("This is some sample content")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("");
                var response = client.Execute(request);

                Assert.Equal("This is some sample content", response.Content);
            }
        }

        static Action<HttpListenerContext> GzipEchoValue(string value)
        {
            return context =>
            {
                context.Response.Headers.Add("Content-encoding", "gzip");

                using (var gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true))
                {
                    gzip.WriteStringUtf8(value);
                }
            };
        }

        static Action<HttpListenerContext> DeflateEchoValue(string value)
        {
            return context =>
            {
                context.Response.Headers.Add("Content-encoding", "deflate");

                using (var gzip = new DeflateStream(context.Response.OutputStream, CompressionMode.Compress, true))
                {
                    gzip.WriteStringUtf8(value);
                }
            };
        }
    }
}
