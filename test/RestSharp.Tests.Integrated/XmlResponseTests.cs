using RestSharp.Interceptors;
using RestSharp.Serializers.Xml;

namespace RestSharp.Tests.Integrated;

public sealed class XmlResponseTests : IDisposable {
    public XmlResponseTests() {
        _server = WireMockServer.Start();

        _server
            .Given(Request.Create().WithPath("/contenttype_odata"))
            .RespondWith(Response.Create().WithCallback(ContentTypeOData));

        _server
            .Given(Request.Create().WithPath("/success"))
            .RespondWith(
                Response
                    .Create()
                    .WithHeader(KnownHeaders.ContentType, ContentType.Xml)
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8" ?>
                        <Response>
                            <Success>
                                <Message>Works!</Message>
                            </Success>
                        </Response>
                        """
                    )
            );
        
        _server
            .Given(Request.Create().WithPath("/error"))
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(400)
                    .WithHeader(KnownHeaders.ContentType, ContentType.Xml)
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8" ?>
                        <Response>
                            <Error>
                                <Message>Not found!</Message>
                            </Error>
                        </Response>
                        """
                    )
            );

        _client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseXmlSerializer());
    }

    public void Dispose() => _server.Dispose();

    readonly WireMockServer _server;
    readonly RestClient     _client;

    [Fact]
    public async Task Handles_Default_Root_Element_On_No_Error() {
        var request = new RestRequest("success") {
            RootElement = "Success"
        };

        var interceptor = new CompatibilityInterceptor {
            OnBeforeDeserialization = resp => {
                if (resp.StatusCode == HttpStatusCode.NotFound) request.RootElement = "Error";
            }
        };
        request.Interceptors = [interceptor];

        var response = await _client.ExecuteAsync<TestResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Handles_Different_Root_Element_On_Http_Error() {
        var request = new RestRequest("error") {
            RootElement = "Success",
            Interceptors = [
                new CompatibilityInterceptor {
                    OnBeforeDeserialization = resp => {
                        if (resp.StatusCode == HttpStatusCode.BadRequest) resp.RootElement = "Error";
                    }
                }
            ]
        };

        var response = await _client.ExecuteAsync<TestResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Data!.Message.Should().Be("Not found!");
    }

    [Fact]
    public async Task ContentType_Additional_Information() {
        var request = new RestRequest("", Method.Post) {
            RequestFormat = DataFormat.Json,
            Resource      = "contenttype_odata"
        };
        request.AddBody("bodyadsodajjd");
        request.AddHeader("X-RequestDigest", "xrequestdigestasdasd");
        request.AddHeader(KnownHeaders.Accept, $"{ContentType.Json}; odata=verbose");
        request.AddHeader(KnownHeaders.ContentType, $"{ContentType.Json}; odata=verbose");

        var response = await _client.ExecuteAsync<TestResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessful.Should().BeTrue();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    static ResponseMessage ContentTypeOData(IRequestMessage request) {
        var contentType      = request.Headers![KnownHeaders.ContentType];
        var hasCorrectHeader = contentType!.Contains($"{ContentType.Json}; odata=verbose");

        var response = new ResponseMessage {
            StatusCode = hasCorrectHeader ? 200 : 400
        };
        return response;
    }
}