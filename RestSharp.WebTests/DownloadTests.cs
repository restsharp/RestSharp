using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace RestSharp.WebTests
{
	public class DownloadTests
	{
		[Fact]
		public void Can_Download_Binary_File_With_ExecuteFile() {
			var request = new RestRequest { BaseUrl = "http://localhost:56976", Action = "Images/Koala.jpg" };
			var client = new RestClient();
			var response = client.DownloadData(request);

			var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Resources\\Koala.jpg");

			Assert.Equal(expected, response);
		}
	}
}