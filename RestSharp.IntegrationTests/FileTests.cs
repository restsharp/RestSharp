using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

		[Fact]
		public void Handles_MultipartFormData_With_File(){
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("upload"){Method = Method.POST}; // non-existent URL

			request.AddFile("Assets\\Koala.jpg");
			request.AddParameter("parameter", "parameter value");

			var response = client.Execute(request);

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}
}
