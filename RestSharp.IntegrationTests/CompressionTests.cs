using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class CompressionTests : TestBase
	{
		[Fact]
		public void Can_Handle_Gzip_Compressed_Content() {
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("Compression/GZip");
			var response = client.Execute(request);

			Assert.Equal("This content is compressed with GZip!", response.Content);
		}

		[Fact]
		public void Can_Handle_Deflate_Compressed_Content() {
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("Compression/Deflate");
			var response = client.Execute(request);
			Assert.Equal("This content is compressed with Deflate!", response.Content);
		}

		[Fact]
		public void Can_Handle_Uncompressed_Content() {
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("Compression/None");
			var response = client.Execute(request);
			Assert.Equal("This content is uncompressed!", response.Content);
		}
	}
}
