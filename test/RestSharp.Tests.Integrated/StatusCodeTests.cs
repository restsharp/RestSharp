using System.Net;
using RestSharp.Serializers;
using RestSharp.Serializers.Xml;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace RestSharp.Tests.Integrated;

public class StatusCodeTests : IDisposable {
    public StatusCodeTests() {
        _server = SimpleServer.Create(UrlToStatusCodeHandler);
        _client = new RestClient(_server.Url, configureSerialization: cfg => cfg.UseXmlSerializer());
    }

    public void Dispose() => _server.Dispose();

    readonly SimpleServer _server;
    readonly RestClient   _client;

    static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url!.Segments.Last());

    [Fact]
    public async Task ContentType_Additional_Information() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

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

    [Fact]
    public async Task Handles_Default_Root_Element_On_No_Error() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

        var request = new RestRequest("success") {
            RootElement = "Success"
        };

        request.OnBeforeDeserialization = resp => {
            if (resp.StatusCode == HttpStatusCode.NotFound) request.RootElement = "Error";
        };

        var response = await _client.ExecuteAsync<TestResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Message.Should().Be("Works!");
    }

    [Fact]
    public async Task Handles_Different_Root_Element_On_Http_Error() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

        var request = new RestRequest("error") {
            RootElement = "Success",
            OnBeforeDeserialization = resp => {
                if (resp.StatusCode == HttpStatusCode.BadRequest) resp.RootElement = "Error";
            }
        };

        var response = await _client.ExecuteAsync<TestResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Data.Message.Should().Be("Not found!");
    }

    [Fact]
    public async Task Handles_GET_Request_404_Error() {
        var request  = new RestRequest("404");
        var response = await _client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reports_1xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("100");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_2xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("204");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeTrue();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Reports_3xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("301");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_4xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("404");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Reports_5xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("503");
        var response = await _client.ExecuteAsync(request);

        response.IsSuccessful.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }
}

public class ResponseHandler {
    void contenttype_odata(HttpListenerContext context) {
        var contentType      = context.Request.Headers[KnownHeaders.ContentType];
        var hasCorrectHeader = contentType!.Contains($"{ContentType.Json}; odata=verbose");
        context.Response.StatusCode = hasCorrectHeader ? 200 : 400;
    }

    void error(HttpListenerContext context) {
        context.Response.StatusCode = 400;
        context.Response.Headers.Add(KnownHeaders.ContentType, ContentType.Xml);

        context.Response.OutputStream.WriteStringUtf8(
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>"
        );
    }

    void errorwithbody(HttpListenerContext context) {
        context.Response.StatusCode = 400;
        context.Response.Headers.Add(KnownHeaders.ContentType, "application/xml");

        context.Response.OutputStream.WriteStringUtf8(
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>"
        );
    }

    void success(HttpListenerContext context)
        => context.Response.OutputStream.WriteStringUtf8(
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>"
        );
}

public class TestResponse {
    public string Message { get; set; }
}
