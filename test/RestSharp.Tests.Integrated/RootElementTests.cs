using RestSharp.Serializers.Xml;

namespace RestSharp.Tests.Integrated; 

public class RootElementTests {
    [Fact]
    public async Task Copy_RootElement_From_Request_To_IWithRootElement_Deserializer() {
        using var server = WireMockServer.Start();

        const string xmlBody =
            """
            <?xml version="1.0" encoding="utf-8" ?>
            <Response>
                <Success>
                    <Message>Works!</Message>
                </Success>
            </Response>
            """;
        server
            .Given(Request.Create().WithPath("/success"))
            .RespondWith(Response.Create().WithBody(xmlBody).WithHeader(KnownHeaders.ContentType, ContentType.Xml));

        using var client = new RestClient(server.Url!, configureSerialization: cfg => cfg.UseXmlSerializer());

        var request = new RestRequest("success") { RootElement = "Success" };

        var response = await client.ExecuteAsync<TestResponse>(request);

        response.Data.Should().NotBeNull();
        response.Data!.Message.Should().Be("Works!");
    }
}