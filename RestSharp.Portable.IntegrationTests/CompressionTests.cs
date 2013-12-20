using System;
using System.IO.Compression;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
    [Trait("Integration", "Compression Tests")]
	public class CompressionTests
	{
        const string baseUrl = "http://localhost:8080/";
        
        [Fact]
        
		public async void Can_Handle_GET_Request_With_Gzip_Compressed_Response()
		{
			using(SimpleServer.Create(baseUrl, GzipEchoValue("This is some gzipped content")))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("/compressed");
				var response = await client.ExecuteAsync(request);

				Assert.Equal("This is some gzipped content", response.Content);
			}
		}

        [Fact]
        
		public async void Can_Handle_GET_Request_With_Deflate_Compressed_Response()
		{
			using(SimpleServer.Create(baseUrl, DeflateEchoValue("This is some deflated content")))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("");
                var response = await client.ExecuteAsync(request);

				Assert.Equal("This is some deflated content", response.Content);
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
				using(var gzip = new DeflateStream(context.Response.OutputStream, CompressionMode.Compress, true))
				{
					gzip.WriteStringUtf8(value);
				}
			};
		}
	}
}
