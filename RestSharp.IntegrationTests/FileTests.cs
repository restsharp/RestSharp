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

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.FileHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("Assets/Koala.jpg");
                byte[] response = client.DownloadData(request);
                byte[] expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");

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

                using (FileStream writer = File.OpenWrite(tempFile))
                {
                    RestClient client = new RestClient(baseUrl);
                    RestRequest request = new RestRequest("Assets/Koala.jpg")
                                          {
                                              ResponseWriter = (responseStream) => responseStream.CopyTo(writer)
                                          };
                    byte[] response = client.DownloadData(request);

                    Assert.Null(response);
                }

                byte[] fromTemp = File.ReadAllBytes(tempFile);
                byte[] expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");

                Assert.AreEqual(expected, fromTemp);
            }
        }
    }
}
