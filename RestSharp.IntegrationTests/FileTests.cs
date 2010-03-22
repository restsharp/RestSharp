using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class FileTests : TestBase
	{
		[Fact]
		public void Handles_Binary_File_Download() {
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("Assets/Koala.jpg");
			var response = client.DownloadData(request);

			var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");
			Assert.Equal(expected, response);
		}
	}
}
