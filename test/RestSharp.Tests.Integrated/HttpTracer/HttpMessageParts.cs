
namespace RestSharp.Tests.Integrated.HttpTracer;

[Flags]
public enum HttpMessageParts {
    Unspecified     = 0,
    RequestBody     = 1 << 1,
    RequestHeaders  = 1 << 2,
    ResponseBody    = 1 << 3,
    ResponseHeaders = 1 << 4,
    RequestCookies  = 1 << 5,

    RequestAll  = RequestBody | RequestHeaders | RequestCookies,
    ResponseAll = ResponseBody | ResponseHeaders,
    All         = ResponseAll | RequestAll
}

[Flags]
public enum JsonFormatting {
    None           = 0,
    IndentRequest  = 1 << 0,
    IndentResponse = 1 << 1
}