using System;
using System.IO;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class FileTests
	{
		[Fact]
		public void Handles_Binary_File_Download()
		{
			const string baseUrl = "http://localhost:8080/";
			using(SimpleServer.Create(baseUrl, Handlers.FileHandler))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("Assets/Koala.jpg");
				var response = client.DownloadData(request);

				var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");
				Assert.Equal(expected, response);
			}
		}
	}
}
