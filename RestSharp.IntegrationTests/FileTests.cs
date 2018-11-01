using System;
using System.IO;
using System.Reflection;
using System.Text;
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

        [Test]
        public void AdvancedResponseWriter_without_ResponseWriter_reads_stream()
        {
            string tag = string.Empty;

            var rr = new RestRequest("Assets/Koala.jpg")
            {
                AdvancedResponseWriter = (stream, context) =>
                {
                    var buf = new byte[16];
                    stream.Read(buf, 0, buf.Length);
                    tag = Encoding.ASCII.GetString(buf, 6, 4);
                }
            };

            _client.Execute(rr);
            Assert.IsTrue("JFIF".CompareTo(tag) == 0);
        }
    }
}