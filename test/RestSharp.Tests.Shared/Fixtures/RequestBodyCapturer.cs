using System.Net;
using RestSharp.Tests.Shared.Extensions;

namespace RestSharp.Tests.Shared.Fixtures;

public class RequestBodyCapturer {
    public const string Resource = "Capture";

    public static string CapturedContentType   { get; set; }
    public static bool   CapturedHasEntityBody { get; set; }
    public static string CapturedEntityBody    { get; set; }
    public static Uri    CapturedUrl           { get; set; }

    // ReSharper disable once UnusedMember.Global
    public static void Capture(HttpListenerContext context) {
        var request = context.Request;

        CapturedContentType   = request.ContentType;
        CapturedHasEntityBody = request.HasEntityBody;
        CapturedEntityBody    = request.InputStream.StreamToString();
        CapturedUrl           = request.Url;
    }
}