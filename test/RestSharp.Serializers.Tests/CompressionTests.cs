using System.Net;
using System.Text;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;
using RestSharp.Serializers.Compression;
using System.IO;

namespace RestSharp.Serializers.Tests
{
    [TestFixture]
    public class CompressionTests
    {
        static readonly Fixture Fixture = new Fixture();

        byte[] _body;

        [Test]
        public void Use_GZip_For_JSon_Requests()
        {
            using var server = HttpServerFixture.StartServer(CaptureBody);
            _body = null;
            var serializer = new CompressionSerializer(new JsonNetSerializer());

            var testData = Fixture.Create<TestClass>();

            var client = new RestClient(server.Url).UseCompression(new JsonNetSerializer());
            var request = new RestRequest().AddJsonBody(testData);

            var expected = testData;

            client.Post(request);



            var actual = serializer.DeserializeFromBytes<TestClass>(_body);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Use_GUnzip_For_JSon_Response()
        {
            var expected = Fixture.Create<TestClass>();


            using var server = HttpServerFixture.StartServer(
                (request, response) =>
                {
                    var serializer = new CompressionSerializer(new JsonNetSerializer());
                    var compressedJsonBytes = serializer.SerializeToBytes(expected);

                    response.ContentType = "application/x-gzip";
                    response.OutputStream.Write(compressedJsonBytes, 0, compressedJsonBytes.Length);
                }
            );

            var client = new RestClient(server.Url).UseCompression(new JsonNetSerializer());

            var actual = client.Get<TestClass>(new RestRequest()).Data;

            actual.Should().BeEquivalentTo(expected);
        }

        void CaptureBody(HttpListenerRequest request, HttpListenerResponse response) {
            MemoryStream ms = new MemoryStream();

            request.InputStream.CopyTo(ms);

            _body = ms.ToArray();
        }
    }
}