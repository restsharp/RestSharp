using System.Net;
using RestSharp.Extensions;

namespace RestSharp; 

[PublicAPI]
public class HttpResponse {
    string? _content;

    public HttpResponse() {
        ResponseStatus = ResponseStatus.None;
        Headers        = new List<HttpHeader>();
        Cookies        = new List<HttpCookie>();
    }

    public string ContentType { get; set; }
    public long ContentLength { get; set; }
    public string ContentEncoding { get; set; }
    public string Content => _content ??= RawBytes.AsString(ContentEncoding);
    public HttpStatusCode StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public byte[] RawBytes { get; set; }
    public Uri ResponseUri { get; set; }
    public string Server { get; set; }
    public IList<HttpHeader> Headers { get; internal set; }
    public IList<HttpCookie> Cookies { get; }
    public ResponseStatus ResponseStatus { get; set; }
    public string ErrorMessage { get; set; }
    public Exception ErrorException { get; set; }
    public Version ProtocolVersion { get; set; }
}