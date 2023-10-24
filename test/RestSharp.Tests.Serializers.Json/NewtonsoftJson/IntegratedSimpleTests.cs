using System.Net;
using System.Text;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Serializers.Json.NewtonsoftJson;

public class IntegratedSimpleTests {
    string _body;

    void CaptureBody(HttpListenerRequest request, HttpListenerResponse response) => _body = request.InputStream.StreamToString();

    static readonly Fixture Fixture = new();

    [Fact]
    public async Task Use_JsonNet_For_Requests() {
        using var server = HttpServerFixture.StartServer(CaptureBody);
        _body = null;
        var serializer = new JsonNetSerializer();

        var testData = Fixture.Create<TestClass>();

        var client  = new RestClient(server.Url, configureSerialization: cfg => cfg.UseNewtonsoftJson());
        var request = new RestRequest().AddJsonBody(testData);

        await client.PostAsync(request);

        var actual = serializer.Deserialize<TestClass>(new RestResponse(request) { Content = _body! });

        actual.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task Use_JsonNet_For_Response() {
        var expected = Fixture.Create<TestClass>();

        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                var serializer = new JsonNetSerializer();

                response.ContentType     = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8(serializer.Serialize(expected)!);
            }
        );

        var client = new RestClient(server.Url, configureSerialization: cfg => cfg.UseNewtonsoftJson());

        var actual = await client.GetAsync<TestClass>(new RestRequest());

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeserilizationFails_IsSuccessful_Should_BeFalse() {
        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                response.StatusCode      = (int)HttpStatusCode.OK;
                response.ContentType     = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8("invalid json");
            }
        );

        var client = new RestClient(server.Url, configureSerialization: cfg => cfg.UseNewtonsoftJson());

        var response = await client.ExecuteAsync<TestClass>(new RestRequest(), default);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task DeserilizationSucceeds_IsSuccessful_Should_BeTrue() {
        var item = Fixture.Create<TestClass>();

        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                var serializer = new JsonNetSerializer();

                response.StatusCode      = (int)HttpStatusCode.OK;
                response.ContentType     = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8(serializer.Serialize(item)!);
            }
        );

        var client = new RestClient(server.Url, configureSerialization: cfg => cfg.UseNewtonsoftJson());

        var response = await client.ExecuteAsync<TestClass>(new RestRequest(), default);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeTrue();
    }
}
