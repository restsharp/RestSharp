using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WireMock.Types;
using WireMock.Util;

namespace RestSharp.Tests.Integrated.Server;

static class WireMockTestServer {
    public static WireMockServer StartTestServer() {
        var server = WireMockServer.Start();

        server
            .Given(Request.Create().WithPath("/echo"))
            .RespondWith(Response.Create().WithCallback(EchoQuery));

        server
            .Given(Request.Create().WithPath("/success").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(new SuccessResponse("Works!")));

        server
            .Given(Request.Create().WithPath("/delete").UsingDelete())
            .RespondWith(Response.Create().WithBodyAsJson(new SuccessResponse("Works!")));

        server
            .Given(Request.Create().WithPath("/content"))
            .RespondWith(Response.Create().WithCallback(EchoJsonBody));

        server
            .Given(Request.Create().WithPath("/post/json").UsingPost())
            .RespondWith(Response.Create().WithCallback(WrapBody));

        server
            .Given(Request.Create().WithPath("/post/data").UsingPost())
            .RespondWith(Response.Create().WithCallback(HandleForm));

        server
            .Given(Request.Create().WithPath("/post/form").UsingPost())
            .RespondWith(Response.Create().WithCallback(WrapForm));

        server
            .Given(Request.Create().WithPath("/timeout"))
            .RespondWith(Response.Create().WithDelay(1000));

        server
            .Given(Request.Create().WithPath("/redirect"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Redirect).WithHeader("Location", "/success"));

        server
            .Given(Request.Create().WithPath("/status").UsingGet())
            .RespondWith(Response.Create().WithCallback(StatusCode));

        server
            .Given(Request.Create().WithPath("/headers"))
            .RespondWith(Response.Create().WithCallback(EchoHeaders));

        return server;
    }

    static ResponseMessage WrapForm(IRequestMessage request) {
        var response = request.BodyData!.BodyAsFormUrlEncoded!["big_string"].Length;
        return CreateJson(new SuccessResponse($"Works! Length: {response}"));
    }

    static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    static ResponseMessage HandleForm(IRequestMessage request) {
        var result = request.BodyData!.BodyAsFormUrlEncoded!.Select(x => new TestServerResponse(x.Key, x.Value));
        return CreateJson(result);
    }

    static ResponseMessage EchoQuery(IRequestMessage request) {
        var query = request.Query!["msg"];
        var msg   = query[0];

        return new ResponseMessage {
            BodyData = new BodyData {
                DetectedBodyType = BodyType.String,
                BodyAsString     = msg
            }
        };
    }

    static ResponseMessage EchoHeaders(IRequestMessage request) {
        var headers = request.Headers!.Select(x => new TestServerResponse(x.Key, x.Value.First()));
        return CreateJson(headers);
    }

    static ResponseMessage EchoJsonBody(IRequestMessage request) => CreateJson(request.BodyAsJson!);

    static ResponseMessage WrapBody(IRequestMessage request) {
        var data = JsonSerializer.Deserialize<TestRequest>(request.Body!, JsonOptions);
        return CreateJson(new TestResponse { Message = data?.Data ?? "" });
    }

    static ResponseMessage StatusCode(IRequestMessage request) {
        var query      = request.Query!["code"];
        var statusCode = int.Parse(query[0]);

        return new ResponseMessage {
            StatusCode = statusCode
        };
    }

    public static ResponseMessage CreateJson(object response)
        => new() {
            BodyData = new BodyData {
                BodyAsJson       = response,
                DetectedBodyType = BodyType.Json
            }
        };
    
    public static async Task<FileMultipartSection?> GetFileSection(this IRequestMessage request, string name) {
        var headerValue = request.Headers![KnownHeaders.ContentType][0];
        var mediaType   = MediaTypeHeaderValue.Parse(headerValue);
        var boundary    = mediaType.Boundary;

        using var stream = new MemoryStream(request.BodyAsBytes!);
        var       reader = new MultipartReader(boundary.Value!, stream);
        reader.HeadersLengthLimit = int.MaxValue;

        FileMultipartSection? fileSection = null;
        while (true) {
            var section = await reader.ReadNextSectionAsync();
            if (section == null) break;
            fileSection = section.AsFileSection();
            if (fileSection == null) continue;
            if (fileSection.Name == name) break;
        }

        return fileSection;
    }
}