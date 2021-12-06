using System.Net;
using RestSharp.IntegrationTests.SampleDeserializers;
using RestSharp.Serialization;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests; 

public class RootElementTests {
    [Fact]
    public void Copy_RootElement_From_Request_To_IWithRootElement_Deserializer() {
        using var server = HttpServerFixture.StartServer("success", Handle);

        var client = new RestClient(server.Url);

        var request = new RestRequest("success") {
            RootElement = "Success"
        };

        var deserializer = new CustomDeserializer();
        client.AddHandler(ContentType.Xml, () => deserializer);
        client.Execute<Response>(request);

        Assert.Equal(request.RootElement, deserializer.RootElement);

        static void Handle(HttpListenerRequest req, HttpListenerResponse response) {
            response.StatusCode = 200;
            response.Headers.Add("Content-Type", ContentType.Xml);

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