using System;
using System.IO;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class FileTests
    {
        [Test]
        public void Handles_Binary_File_Download()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using(SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.FileHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("Assets/Koala.jpg");
                var response = client.DownloadData(request);
                var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");

                Assert.AreEqual(expected, response);
            }
        }

        [Test]
        public void Writes_Response_To_Stream()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, Handlers.FileHandler))
            {
                string tempFile = Path.GetTempFileName();

                using (var writer = File.OpenWrite(tempFile))
                {
                    var client = new RestClient(baseUrl);
                    var request = new RestRequest("Assets/Koala.jpg")
                                  {
                                      ResponseWriter = (responseStream) => responseStream.CopyTo(writer)
                                  };
                    var response = client.DownloadData(request);

                    Assert.Null(response);
                }

                var fromTemp = File.ReadAllBytes(tempFile);
                var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");

                Assert.AreEqual(expected, fromTemp);
            }
        }
    }
}
