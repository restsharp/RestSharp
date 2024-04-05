namespace RestSharp.Tests.Shared.Fixtures;

public class RequestBodyCapturer {
    public const string Resource = "/capture";

    public string ContentType { get; private set; }
    public bool   HasBody     { get; private set; }
    public string Body        { get; private set; }
    public Uri    Url         { get; private set; }

    public bool CaptureBody(string content) {
        Body    = content;
        HasBody = !string.IsNullOrWhiteSpace(content);
        return true;
    }

    public bool CaptureHeaders(IDictionary<string, string[]> headers) {
        if (headers.TryGetValue("Content-Type", out var contentType)) {
            ContentType = contentType[0];
        }

        return true;
    }

    public bool CaptureUrl(string url) {
        Url = new Uri(url);
        return true;
    }
}