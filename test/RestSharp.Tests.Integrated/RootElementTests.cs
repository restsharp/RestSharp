using System.Net;
using RestSharp.Serializers.Xml;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated; 

public class RootElementTests {
    [Fact]
    public async Task Copy_RootElement_From_Request_To_IWithRootElement_Deserializer() {
        using var server = HttpServerFixture.StartServer("success", Handle);

        var client = new RestClient(server.Url).UseXmlSerializer();

        var request = new RestRequest("success") { RootElement = "Success" };

        var response = await client.ExecuteAsync<TestResponse>(request);

        response.Data.Should().NotBeNull();
        response.Data!.Message.Should().Be("Works!");

        static void Handle(HttpListenerRequest req, HttpListenerResponse response) {
            response.StatusCode = 200;
            response.Headers.Add(KnownHeaders.ContentType, Serializers.ContentType.Xml);

            response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>"
            );
        }
    }
}