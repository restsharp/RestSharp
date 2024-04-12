using System.Text.Json;
using WireMock.Settings;
using WireMock.Types;
using WireMock.Util;

namespace RestSharp.Tests.Integrated.Server;

// ReSharper disable once ClassNeverInstantiated.Global
public class WireMockTestServer : WireMockServer {
    public WireMockTestServer() : base(new WireMockServerSettings { Port = 0, UseHttp2 = false, UseSSL = false }) {
        Given(Request.Create().WithPath("/echo"))
            .RespondWith(Response.Create().WithCallback(EchoQuery));

        Given(Request.Create().WithPath("/success").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(new SuccessResponse("Works!")));

        Given(Request.Create().WithPath("/delete").UsingDelete())
            .RespondWith(Response.Create().WithBodyAsJson(new SuccessResponse("Works!")));

        Given(Request.Create().WithPath("/content"))
            .RespondWith(Response.Create().WithCallback(EchoJsonBody));

        Given(Request.Create().WithPath("/post/json").UsingPost())
            .RespondWith(Response.Create().WithCallback(WrapBody));

        Given(Request.Create().WithPath("/post/data").UsingPost())
            .RespondWith(Response.Create().WithCallback(HandleForm));

        Given(Request.Create().WithPath("/post/form").UsingPost())
            .RespondWith(Response.Create().WithCallback(WrapForm));

        Given(Request.Create().WithPath("/timeout"))
            .RespondWith(Response.Create().WithDelay(1000));

        Given(Request.Create().WithPath("/redirect"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Redirect).WithHeader("Location", "/success"));

        Given(Request.Create().WithPath("/status").UsingGet())
            .RespondWith(Response.Create().WithCallback(StatusCode));

        Given(Request.Create().WithPath("/headers"))
            .RespondWith(Response.Create().WithCallback(EchoHeaders));
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

}