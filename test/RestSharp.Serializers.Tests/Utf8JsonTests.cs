using System.Net;
using System.Text;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Serializers.Utf8Json;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Serializers.Tests
{
    [TestFixture]
    public class Utf8JsonTests
    {
        static readonly Fixture Fixture = new Fixture();

        byte[] _body;

        [Test]
        public void Use_Utf8Json_For_Requests()
        {
            using var server = HttpServerFixture.StartServer(CaptureBody);
            _body = null;
            var serializer = new Utf8JsonSerializer();

            var testData = Fixture.Create<TestClass>();

            var client  = new RestClient(server.Url).UseUtf8Json();
            var request = new RestRequest().AddJsonBody(testData);

            var expected = testData;

           var a=  client.Post(request);

            var actual = serializer.Deserialize<TestClass>(new RestResponse {RawBytes = _body});

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Use_Utf8Json_For_Response()
        {
            var expected = Fixture.Create<TestClass>();

            using var server = HttpServerFixture.StartServer(
                (request, response) =>
                {
                    var serializer = new Utf8JsonSerializer();

                    response.ContentType     = "application/json";
                    response.ContentEncoding = Encoding.UTF8;
                    response.OutputStream.WriteStringUtf8(serializer.Serialize(expected));
                }
            );

            var client = new RestClient(server.Url).UseUtf8Json();

            var actual = client.Get<TestClass>(new RestRequest()).Data;

            actual.Should().BeEquivalentTo(expected);
        }

        void CaptureBody(HttpListenerRequest request, HttpListenerResponse response) => _body = request.InputStream.StreamToBytes();
    }
}
