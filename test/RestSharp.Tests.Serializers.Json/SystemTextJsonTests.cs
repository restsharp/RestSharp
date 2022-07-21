using System.Net;
using System.Text;
using RestSharp.Serializers.Json;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Serializers.Json; 

public class SystemTextJsonTests {
    static readonly Fixture Fixture = new();

    string _body;

    [Fact]
    public async Task Use_JsonNet_For_Requests() {
        using var server = HttpServerFixture.StartServer(CaptureBody);
        _body = null;
        var serializer = new SystemTextJsonSerializer();

        var testData = Fixture.Create<TestClass>();

        var client  = new RestClient(server.Url);
        var request = new RestRequest().AddJsonBody(testData);

        await client.PostAsync(request);

        var actual = serializer.Deserialize<TestClass>(new RestResponse { Content = _body });

        actual.Should().BeEquivalentTo(testData);

        void CaptureBody(HttpListenerRequest req, HttpListenerResponse response) => _body = req.InputStream.StreamToString();
    }

    [Fact]
    public async Task Use_JsonNet_For_Response() {
        var expected = Fixture.Create<TestClass>();

        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                var serializer = new SystemTextJsonSerializer();

                response.ContentType     = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8(serializer.Serialize(expected)!);
            }
        );

        var client = new RestClient(server.Url).UseSystemTextJson();

        var actual = await client.GetAsync<TestClass>(new RestRequest());

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeserilizationFails_IsSuccessfull_Should_BeFalse() {
        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8("invalid json");
            }
        );

        var client = new RestClient(server.Url).UseSystemTextJson();

        var response = await client.ExecuteAsync<TestClass>(new RestRequest());

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task DeserilizationSucceeds_IsSuccessfull_Should_BeTrue() {
        var item = Fixture.Create<TestClass>();

        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                var serializer = new SystemTextJsonSerializer();

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8(serializer.Serialize(item)!);
            }
        );

        var client = new RestClient(server.Url).UseSystemTextJson();

        var response = await client.ExecuteAsync<TestClass>(new RestRequest());

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeTrue();
    }
}