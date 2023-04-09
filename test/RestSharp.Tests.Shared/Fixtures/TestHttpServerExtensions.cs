using System.Net;

namespace RestSharp.Tests.Shared.Fixtures; 

public static class TestHttpServerExtensions {
    static readonly Dictionary<HttpListenerRequest, string> RequestContent = new();

    internal static void ClearContent(this HttpListenerRequest request) => RequestContent.Remove(request);

    public static HttpListenerResponse ContentType(this HttpListenerResponse response, string contentType) {
        response.ContentType = contentType;
        return response;
    }

    public static HttpListenerResponse StatusCode(this HttpListenerResponse response, int statusCode) {
        response.StatusCode = statusCode;
        return response;
    }
}