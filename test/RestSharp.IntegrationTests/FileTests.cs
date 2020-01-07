using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class FileTests
    {
        [TearDown]
        public void ShutdownServer() => _server.Dispose();

        [SetUp]
        public void CreateClient()
        {
            _server = HttpServerFixture.StartServer("Assets/Koala.jpg", FileHandler);
            _client = new RestClient(_server.Url);
        }

        void FileHandler(HttpListenerRequest request, HttpListenerResponse response)
        {
            var pathToFile = Path.Combine(
                _path,
                Path.Combine(
                    request.Url.Segments.Select(s => s.Replace("/", "")).ToArray()
                )
            );

            using var reader = new StreamReader(pathToFile);

            reader.BaseStream.CopyTo(response.OutputStream);
        }

        HttpServerFixture _server;
        RestClient        _client;
        readonly string   _path = AppDomain.CurrentDomain.BaseDirectory;

        [Test]
        public void AdvancedResponseWriter_without_ResponseWriter_reads_stream()
        {
            var tag = string.Empty;

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
            Assert.IsTrue(string.Compare("JFIF", tag, StringComparison.Ordinal) == 0);
        }

        [Test]
        public void Handles_Binary_File_Download()
        {
            var request  = new RestRequest("Assets/Koala.jpg");
            var response = _client.DownloadData(request);
            var expected = File.ReadAllBytes(Path.Combine(_path, "Assets", "Koala.jpg"));

            Assert.AreEqual(expected, response);
        }

        [Test]
        public void Writes_Response_To_Stream()
        {
            var tempFile = Path.GetTempFileName();

            using (var writer = File.OpenWrite(tempFile))
            {
                var request = new RestRequest("Assets/Koala.jpg")
                {
                    ResponseWriter = responseStream => responseStream.CopyTo(writer)
                };
                var response = _client.DownloadData(request);

                Assert.Null(response);
            }

            var fromTemp = File.ReadAllBytes(tempFile);
            var expected = File.ReadAllBytes(Path.Combine(_path, "Assets", "Koala.jpg"));

            Assert.AreEqual(expected, fromTemp);
        }
    }
}