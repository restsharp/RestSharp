using System.Net;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests;

public class StatusCodeTests : IDisposable {
    public StatusCodeTests() {
        _server = SimpleServer.Create(UrlToStatusCodeHandler);
        _client = new RestClient(_server.Url);
    }

    public void Dispose() => _server.Dispose();

    readonly SimpleServer _server;
    readonly RestClient   _client;

    static void UrlToStatusCodeHandler(HttpListenerContext obj) => obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());

    [Fact]
    public void ContentType_Additional_Information() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

        var request = new RestRequest(Method.Post) {
            RequestFormat = DataFormat.Json,
            Resource      = "contenttype_odata"
        };
        request.AddBody("bodyadsodajjd");
        request.AddHeader("X-RequestDigest", "xrequestdigestasdasd");
        request.AddHeader("Accept", $"{ContentType.Json}; odata=verbose");
        request.AddHeader("Content-Type", $"{ContentType.Json}; odata=verbose");

        var response = _client.Execute<Response>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Handles_Default_Root_Element_On_No_Error() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

        var request = new RestRequest("success") {
            RootElement = "Success"
        };

        request.OnBeforeDeserialization = resp => {
            if (resp.StatusCode == HttpStatusCode.NotFound) request.RootElement = "Error";
        };

        var response = _client.Execute<Response>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Works!", response.Data.Message);
    }

    [Fact]
    public void Handles_Different_Root_Element_On_Http_Error() {
        _server.SetHandler(Handlers.Generic<ResponseHandler>());

        var request = new RestRequest("error") {
            RootElement = "Success"
        };

        request.OnBeforeDeserialization =
            resp => {
                if (resp.StatusCode == HttpStatusCode.BadRequest) request.RootElement = "Error";
            };

        var response = _client.Execute<Response>(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Not found!", response.Data.Message);
    }

    [Fact]
    public void Handles_GET_Request_404_Error() {
        var request  = new RestRequest("404");
        var response = _client.Execute(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Not sure why this hangs")]
    public void Reports_1xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("100");
        var response = _client.Execute(request);

        Assert.False(response.IsSuccessful);
    }

    [Fact]
    public void Reports_2xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("204");
        var response = _client.Execute(request);

        Assert.True(response.IsSuccessful);
    }

    [Fact]
    public void Reports_3xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("301");
        var response = _client.Execute(request);

        Assert.False(response.IsSuccessful);
    }

    [Fact]
    public void Reports_4xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("404");
        var response = _client.Execute(request);

        Assert.False(response.IsSuccessful);
    }

    [Fact]
    public void Reports_5xx_Status_Code_Success_Accurately() {
        var request  = new RestRequest("503");
        var response = _client.Execute(request);

        Assert.False(response.IsSuccessful);
    }
}

public class ResponseHandler {
    void contenttype_odata(HttpListenerContext context) {
        var hasCorrectHeader = context.Request.Headers["Content-Type"] == $"{ContentType.Json}; odata=verbose";
        context.Response.StatusCode = hasCorrectHeader ? 200 : 400;
    }

    void error(HttpListenerContext context) {
        context.Response.StatusCode = 400;
        context.Response.Headers.Add("Content-Type", ContentType.Xml);

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
        context.Response.Headers.Add("Content-Type", "application/xml");

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

public class Response {
    public string Message { get; set; }
}