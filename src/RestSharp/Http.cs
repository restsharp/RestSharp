using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp;

/// <summary>
/// HttpWebRequest wrapper
/// </summary>
public partial class Http {
    const string LineBreak = "\r\n";

    public string FormBoundary { get; } = $"---------{Guid.NewGuid().ToString().ToUpperInvariant()}";

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    static readonly Regex AddRangeRegex = new("(\\w+)=(\\d+)-(\\d+)$");

    readonly Dictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions = new(StringComparer.OrdinalIgnoreCase);

    public Http() {
        AddSharedHeaderActions();
        AddSyncHeaderActions();

        void AddSyncHeaderActions() {
            _restrictedHeaderActions.Add("Connection", (r, v) => r.KeepAlive = v.ToLowerInvariant().Contains("keep-alive"));

            _restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));

            _restrictedHeaderActions.Add("Expect", (r, v) => r.Expect = v);

            _restrictedHeaderActions.Add(
                "If-Modified-Since",
                (r, v) => r.IfModifiedSince = Convert.ToDateTime(v, CultureInfo.InvariantCulture)
            );

            _restrictedHeaderActions.Add("Referer", (r, v) => r.Referer = v);

            _restrictedHeaderActions.Add(
                "Transfer-Encoding",
                (r, v) => {
                    r.TransferEncoding = v;
                    r.SendChunked      = true;
                }
            );

            _restrictedHeaderActions.Add("User-Agent", (r, v) => r.UserAgent = v);
        }

        void AddSharedHeaderActions() {
            _restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);

            _restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);

            _restrictedHeaderActions.Add(
                "Date",
                (r, v) => {
                    if (DateTime.TryParse(v, out var parsed)) r.Date = parsed;
                }
            );

            _restrictedHeaderActions.Add("Host", (r, v) => r.Host = v);

            _restrictedHeaderActions.Add("Range", AddRange);

            static void AddRange(HttpWebRequest r, string range) {
                var m = AddRangeRegex.Match(range);

                if (!m.Success) return;

                var rangeSpecifier = m.Groups[1].Value;
                var from           = Convert.ToInt64(m.Groups[2].Value);
                var to             = Convert.ToInt64(m.Groups[3].Value);

                r.AddRange(rangeSpecifier, from, to);
            }
        }
    }

    /// <summary>
    /// True if this HTTP request has any HTTP parameters
    /// </summary>
    protected bool HasParameters => Parameters.Any();

    /// <summary>
    /// True if this HTTP request has any HTTP cookies
    /// </summary>
    protected bool HasCookies => Cookies.Any();

    /// <summary>
    /// True if a request body has been specified
    /// </summary>
    protected bool HasBody => RequestBodyBytes != null || !string.IsNullOrEmpty(RequestBody);

    /// <summary>
    /// True if files have been set to be uploaded
    /// </summary>
    protected bool HasFiles => Files.Any();

    internal Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    public bool AutomaticDecompression { get; set; }

    /// <summary>
    /// Always send a multipart/form-data request - even when no Files are present.
    /// </summary>
    public bool AlwaysMultipartFormData { get; set; }

    public string UserAgent { get; set; } = null!;

    public int Timeout { get; set; }

    public int ReadWriteTimeout { get; set; }

    public ICredentials? Credentials { get; set; }

    public CookieContainer? CookieContainer { get; set; }

    public Action<Stream, HttpResponse> AdvancedResponseWriter { get; set; } = null!;

    public Action<Stream> ResponseWriter { get; set; } = null!;

    public IList<HttpFile> Files { get; internal set; } = null!;

    public bool FollowRedirects { get; set; }

    public bool Pipelined { get; set; }

    public X509CertificateCollection? ClientCertificates { get; set; }

    public int? MaxRedirects { get; set; }

    public bool UseDefaultCredentials { get; set; }

    public string ConnectionGroupName { get; set; } = null!;

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public IList<HttpHeader> Headers { get; internal set; } = null!;

    public IList<HttpParameter> Parameters { get; internal set; } = null!;

    public IList<HttpCookie> Cookies { get; internal set; } = null!;

    public string? RequestBody { get; set; }

    public string RequestContentType { get; set; } = null!;

    public byte[]? RequestBodyBytes { get; set; }

    public Uri Url { get; set; } = null!;

    public string? Host { get; set; }

    public IList<DecompressionMethods> AllowedDecompressionMethods { get; set; } = null!;

    public bool PreAuthenticate { get; set; }

    // public bool UnsafeAuthenticatedConnectionSharing { get; set; }

    public IWebProxy? Proxy { get; set; }

    public RequestCachePolicy? CachePolicy { get; set; }

    /// <summary>
    /// Callback function for handling the validation of remote certificates.
    /// </summary>
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

    public Action<HttpWebRequest>? WebRequestConfigurator { get; set; }

    public bool ThrowOnAnyError { get; set; }

    protected virtual HttpWebRequest? CreateWebRequest(Uri url) => null;

    static HttpWebRequest CreateRequest(Uri uri)
        => (HttpWebRequest)WebRequest.Create(uri);

    string GetMultipartFileHeader(HttpFile file)
        => $"--{FormBoundary}{LineBreak}Content-Disposition: form-data; name=\"{file.Name}\";" +
            $" filename=\"{file.FileName}\"{LineBreak}" +
            $"Content-Type: {file.ContentType ?? "application/octet-stream"}{LineBreak}{LineBreak}";

    string GetMultipartFormData(HttpParameter param) {
        var format = param.Name == RequestContentType
            ? "--{0}{3}Content-Type: {4}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}"
            : "--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}";

        return string.Format(format, FormBoundary, param.Name, param.Value, LineBreak, param.ContentType!);
    }

    string GetMultipartFooter()
        => $"--{FormBoundary}--{LineBreak}";

    void PreparePostBody(WebRequest webRequest) {
        var needsContentType = string.IsNullOrEmpty(webRequest.ContentType);

        if (HasFiles || AlwaysMultipartFormData) {
            if (needsContentType) {
                webRequest.ContentType = GetMultipartFormContentType();
            }
            else if (!webRequest.ContentType.Contains("boundary")) {
                webRequest.ContentType = webRequest.ContentType + "; boundary=" + FormBoundary;
            }
        }
        else if (HasBody) {
            if (needsContentType) webRequest.ContentType = RequestContentType;
        }
        else if (HasParameters) {
            if (needsContentType) webRequest.ContentType = "application/x-www-form-urlencoded";
            RequestBody = EncodeParameters();
        }

        string EncodeParameters()
            => string.Join("&", Parameters.Select(p => $"{Encode(p.Name)}={Encode(p.Value)}"));

        string GetMultipartFormContentType()
            => $"multipart/form-data; boundary={FormBoundary}";
    }

    void WriteMultipartFormData(Stream requestStream) {
        foreach (var param in Parameters) requestStream.WriteString(GetMultipartFormData(param), Encoding);

        foreach (var file in Files) {
            // Add just the first part of this param, since we will write the file data directly to the Stream
            requestStream.WriteString(GetMultipartFileHeader(file), Encoding);

            // Write the file data directly to the Stream, rather than serializing it to a string.
            file.Writer(requestStream);
            requestStream.WriteString(LineBreak, Encoding);
        }

        requestStream.WriteString(GetMultipartFooter(), Encoding);
    }

    HttpResponse ExtractResponseData(HttpWebResponse webResponse) {
        var response = new HttpResponse {
            ContentEncoding   = webResponse.ContentEncoding,
            Server            = webResponse.Server,
            ProtocolVersion   = webResponse.ProtocolVersion,
            ContentType       = webResponse.ContentType,
            ContentLength     = webResponse.ContentLength,
            StatusCode        = webResponse.StatusCode,
            StatusDescription = webResponse.StatusDescription,
            ResponseUri       = webResponse.ResponseUri,
            ResponseStatus    = ResponseStatus.Completed
        };

        if (webResponse.Cookies != null)
            foreach (Cookie cookie in webResponse.Cookies) {
                response.Cookies.Add(
                    new HttpCookie {
                        Comment    = cookie.Comment,
                        CommentUri = cookie.CommentUri,
                        Discard    = cookie.Discard,
                        Domain     = cookie.Domain,
                        Expired    = cookie.Expired,
                        Expires    = cookie.Expires,
                        HttpOnly   = cookie.HttpOnly,
                        Name       = cookie.Name,
                        Path       = cookie.Path,
                        Port       = cookie.Port,
                        Secure     = cookie.Secure,
                        TimeStamp  = cookie.TimeStamp,
                        Value      = cookie.Value,
                        Version    = cookie.Version
                    }
                );
            }

        response.Headers = webResponse.Headers.AllKeys
            .Select(x => new HttpHeader(x, webResponse.Headers[x]))
            .ToList();

        var webResponseStream = webResponse.GetResponseStream();

        if (webResponseStream != null)
            ProcessResponseStream();

        webResponse.Close();
        return response;

        void ProcessResponseStream() {
            if (AdvancedResponseWriter != null) {
                AdvancedResponseWriter(webResponseStream, response);
            }
            else {
                if (ResponseWriter == null)
                    response.RawBytes = webResponseStream.ReadAsBytes();
                else
                    ResponseWriter(webResponseStream);
            }
        }
    }
}