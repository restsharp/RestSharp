using System.Net;
using RestSharp.IntegrationTests.SampleDeserializers;
using RestSharp.Serializers.Xml;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class RootElementTests {
    [Fact]
    public async Task Copy_RootElement_From_Request_To_IWithRootElement_Deserializer() {
        using var server = HttpServerFixture.StartServer("success", Handle);

        var client = new RestClient(server.Url);

        var request = new RestRequest("success") { RootElement = "Success" };

        var deserializer   = new CustomDeserializer();
        var restSerializer = new XmlRestSerializer(new XmlSerializer(), deserializer);
        client.UseSerializer(() => restSerializer);
        
        await client.ExecuteAsync<Response>(request);

        Assert.Equal(request.RootElement, deserializer.RootElement);

        static void Handle(HttpListenerRequest req, HttpListenerResponse response) {
            response.StatusCode = 200;
            response.Headers.Add("Content-Type", Serializers.ContentType.Xml);

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