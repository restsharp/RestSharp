using System;
using System.IO;
using RestSharp.IntegrationTests.Helpers;
//using Xunit;
using NUnit;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RestSharp.IntegrationTests
{
    //[Trait("Integration", "File Tests")]
    [Category("Integation [File Tests]")]
	public class FileTests
	{
        const string baseUrl = "http://localhost:8080/";
        
        //[Fact]              
        [Test]
        public async void Can_Handle_Binary_File_Download()
		{
			using (SimpleServer.Create(baseUrl, Handlers.FileHandler))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("Assets/Koala.jpg");
				var response = await client.DownloadDataAsync(request);

                byte[] expected = await LoadSourceFile();
				Assert.AreEqual(expected, response);
			}
		}

        //[Fact]
        [Test]
        public async void Can_Write_Response_To_Stream()
        {
            using (SimpleServer.Create(baseUrl, Handlers.FileHandler))
            {
                string tempFile = Path.GetTempFileName();
                using (var writer = File.OpenWrite(tempFile))
                {
                    var client = new RestClient(baseUrl);
                    var request = new RestRequest("Assets/Koala.jpg");
                    request.ResponseWriter = (responseStream) => responseStream.CopyTo(writer);
                    var response = await client.DownloadDataAsync(request);
                    Assert.Null(response);
                }
                var fromTemp = File.ReadAllBytes(tempFile);
                var expected = await LoadSourceFile();
                Assert.AreEqual(expected, fromTemp);
            }
        }

        public static async Task<byte[]> LoadSourceFile()
        {
            byte[] expected = null;
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("RestSharp.IntegrationTests.Assets.Koala.jpg"))
            {
                expected = new byte[stream.Length];
                await stream.ReadAsync(expected, 0, (int)stream.Length);
            }

            return expected;
        }
	}
}