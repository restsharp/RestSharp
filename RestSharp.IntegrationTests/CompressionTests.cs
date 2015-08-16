using System;
using System.IO.Compression;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class CompressionTests
    {
        [Test]
        public void Can_Handle_Gzip_Compressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, GzipEchoValue("This is some gzipped content")))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");
                IRestResponse response = client.Execute(request);

                Assert.AreEqual("This is some gzipped content", response.Content);
            }
        }

        [Test]
        public void Can_Handle_Deflate_Compressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, DeflateEchoValue("This is some deflated content")))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");
                IRestResponse response = client.Execute(request);

                Assert.AreEqual("This is some deflated content", response.Content);
            }
        }

        [Test]
        public void Can_Handle_Uncompressed_Content()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.EchoValue("This is some sample content")))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("");
                IRestResponse response = client.Execute(request);

                Assert.AreEqual("This is some sample content", response.Content);
            }
        }

        private static Action<HttpListenerContext> GzipEchoValue(string value)
        {
            return context =>
                   {
                       context.Response.Headers.Add("Content-encoding", "gzip");

                       using (GZipStream gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true))
                       {
                           gzip.WriteStringUtf8(value);
                       }
                   };
        }

        private static Action<HttpListenerContext> DeflateEchoValue(string value)
        {
            return context =>
                   {
                       context.Response.Headers.Add("Content-encoding", "deflate");

                       using (DeflateStream gzip = new DeflateStream(context.Response.OutputStream, CompressionMode.Compress, true))
                       {
                           gzip.WriteStringUtf8(value);
                       }
                   };
        }
    }
}
