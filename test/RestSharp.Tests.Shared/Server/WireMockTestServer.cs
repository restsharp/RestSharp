using System.Net;
using System.Text.Json;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace RestSharp.Tests.Shared.Server;

// ReSharper disable once ClassNeverInstantiated.Global
public class WireMockTestServer : WireMockServer {
    public WireMockTestServer() : base(new() { Port = 0, UseHttp2 = false, UseSSL = false }) {
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

        Given(Request.Create().WithPath("/redirect-countdown"))
            .RespondWith(Response.Create().WithCallback(RedirectCountdown));

        Given(Request.Create().WithPath("/redirect-with-status"))
            .RespondWith(Response.Create().WithCallback(RedirectWithStatus));

        Given(Request.Create().WithPath("/echo-request"))
            .RespondWith(Response.Create().WithCallback(EchoRequest));
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

    static ResponseMessage RedirectCountdown(IRequestMessage request) {
        var n = 1;

        if (request.Query != null && request.Query.TryGetValue("n", out var nValues)) {
            n = int.Parse(nValues[0]);
        }

        if (n <= 1) {
            return CreateJson(new SuccessResponse("Done!"));
        }

        return new ResponseMessage {
            StatusCode = (int)HttpStatusCode.TemporaryRedirect,
            Headers = new Dictionary<string, WireMockList<string>> {
                ["Location"] = new($"/redirect-countdown?n={n - 1}")
            }
        };
    }

    static ResponseMessage RedirectWithStatus(IRequestMessage request) {
        var status = 302;
        var url    = "/echo-request";

        if (request.Query != null) {
            if (request.Query.TryGetValue("status", out var statusValues)) {
                status = int.Parse(statusValues[0]);
            }

            if (request.Query.TryGetValue("url", out var urlValues)) {
                url = urlValues[0];
            }
        }

        return new ResponseMessage {
            StatusCode = status,
            Headers = new Dictionary<string, WireMockList<string>> {
                ["Location"] = new(url)
            }
        };
    }

    static ResponseMessage EchoRequest(IRequestMessage request) {
        var headers = request.Headers?
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Value))
            ?? new Dictionary<string, string>();

        return CreateJson(new {
            Method  = request.Method,
            Headers = headers,
            Body    = request.Body ?? ""
        });
    }

    public static ResponseMessage CreateJson(object response)
        => new() {
            BodyData = new BodyData {
                BodyAsJson       = response,
                DetectedBodyType = BodyType.Json
            }
        };
}

public record TestRequest(string Data, int Number);