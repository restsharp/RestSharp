using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class FileTests
    {
        private Uri _baseUrl;
        private SimpleServer _server;
        private RestClient _client;
        private string _path;

        [OneTimeSetUp]
        public void SetupServer()
        {
            _baseUrl = new Uri("http://localhost:8888/");
            _path = AppDomain.CurrentDomain.BaseDirectory;
        }

        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        [SetUp]
        public void CreateClient()
        {
            _client = new RestClient(_baseUrl);
            _server = SimpleServer.Create(_baseUrl.AbsoluteUri, c => Handlers.FileHandler(c, _path));
        }

        [Test]
        public void Handles_Binary_File_Download()
        {
            RestRequest request = new RestRequest("Assets/Koala.jpg");
            byte[] response = _client.DownloadData(request);
            byte[] expected = File.ReadAllBytes(_path + "\\Assets\\Koala.jpg");

            Assert.AreEqual(expected, response);
        }

        [Test]
        public void Writes_Response_To_Stream()
        {
            string tempFile = Path.GetTempFileName();

            using (FileStream writer = File.OpenWrite(tempFile))
            {
                RestRequest request = new RestRequest("Assets/Koala.jpg")
                {
                    ResponseWriter = (responseStream) => responseStream.CopyTo(writer)
                };
                byte[] response = _client.DownloadData(request);

                Assert.Null(response);
            }

            byte[] fromTemp = File.ReadAllBytes(tempFile);
            byte[] expected = File.ReadAllBytes(_path + "\\Assets\\Koala.jpg");

            Assert.AreEqual(expected, fromTemp);
        }
    }
}