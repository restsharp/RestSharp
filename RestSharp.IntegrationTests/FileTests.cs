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
			using (SimpleServer.Create(baseUrl, Handlers.FileHandler))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("Assets/Koala.jpg");
				var response = client.DownloadData(request);

				var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");
				Assert.Equal(expected, response);
			}
		}

		[Fact]
		public void Writes_Response_To_Stream()
		{
			const string baseUrl = "http://localhost:8080/";
			using (SimpleServer.Create(baseUrl, Handlers.FileHandler))
			{
				string tempFile = Path.GetTempFileName();
				using (var writer = File.OpenWrite(tempFile))
				{
					var client = new RestClient(baseUrl);
					var request = new RestRequest("Assets/Koala.jpg");
					request.ResponseWriter = (responseStream) => responseStream.CopyTo(writer);
					var response = client.DownloadData(request);
					Assert.Null(response);
				}
				var fromTemp = File.ReadAllBytes(tempFile);
				var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");
				Assert.Equal(expected, fromTemp);
			}
		}
	}
}