#region License

//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    ///     HttpWebRequest wrapper
    /// </summary>
    public partial class Http : IHttp
    {
        const string LineBreak = "\r\n";

        const string FormBoundary = "-----------------------------28947758029299";

        static readonly Regex AddRangeRegex = new Regex("(\\w+)=(\\d+)-(\\d+)$");

        readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public Http()
        {
            _restrictedHeaderActions =
                new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase);

            AddSharedHeaderActions();
            AddSyncHeaderActions();

            void AddSyncHeaderActions()
            {
                _restrictedHeaderActions.Add("Connection", (r, v) => { r.KeepAlive = v.ToLower().Contains("keep-alive"); });
                _restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
                _restrictedHeaderActions.Add("Expect", (r, v) => r.Expect                = v);

                _restrictedHeaderActions.Add(
                    "If-Modified-Since",
                    (r, v) => r.IfModifiedSince = Convert.ToDateTime(v, CultureInfo.InvariantCulture)
                );
                _restrictedHeaderActions.Add("Referer", (r, v) => r.Referer = v);

                _restrictedHeaderActions.Add(
                    "Transfer-Encoding", (r, v) =>
                    {
                        r.TransferEncoding = v;
                        r.SendChunked      = true;
                    }
                );
                _restrictedHeaderActions.Add("User-Agent", (r, v) => r.UserAgent = v);
            }

            void AddSharedHeaderActions()
            {
                _restrictedHeaderActions.Add("Accept", (r, v) => r.Accept            = v);
                _restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);

                _restrictedHeaderActions.Add(
                    "Date", (r, v) =>
                    {
                        if (DateTime.TryParse(v, out var parsed))
                            r.Date = parsed;
                    }
                );

                _restrictedHeaderActions.Add("Host", (r, v) => r.Host = v);

                _restrictedHeaderActions.Add("Range", AddRange);

                static void AddRange(HttpWebRequest r, string range)
                {
                    var m = AddRangeRegex.Match(range);

                    if (!m.Success)
                        return;

                    var rangeSpecifier = m.Groups[1].Value;
                    var from           = Convert.ToInt64(m.Groups[2].Value);
                    var to             = Convert.ToInt64(m.Groups[3].Value);

                    r.AddRange(rangeSpecifier, from, to);
                }
            }
        }

        /// <summary>
        ///     True if this HTTP request has any HTTP parameters
        /// </summary>
        protected bool HasParameters => Parameters.Any();

        /// <summary>
        ///     True if this HTTP request has any HTTP cookies
        /// </summary>
        protected bool HasCookies => Cookies.Any();

        /// <summary>
        ///     True if a request body has been specified
        /// </summary>
        protected bool HasBody => RequestBodyBytes != null || !string.IsNullOrEmpty(RequestBody);

        /// <summary>
        ///     True if files have been set to be uploaded
        /// </summary>
        protected bool HasFiles => Files.Any();

        internal Func<string, string> Encode { get; set; } = s => s.UrlEncode();

        /// <summary>
        ///     Enable or disable automatic gzip/deflate decompression
        /// </summary>
        public bool AutomaticDecompression { get; set; }

        /// <summary>
        ///     Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        public bool AlwaysMultipartFormData { get; set; }

        /// <summary>
        ///     UserAgent to be sent with request
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to be used for the request
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     The number of milliseconds before the writing or reading times out.
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        ///     System.Net.ICredentials to be sent with request
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        ///     The System.Net.CookieContainer to be used for the request
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        ///     The delegate to use to write the response instead of reading into RawBytes
        ///     Here you can also check the request details
        /// </summary>
        public Action<Stream, IHttpResponse> AdvancedResponseWriter { get; set; }

        /// <summary>
        ///     The delegate to use to write the response instead of reading into RawBytes
        /// </summary>
        public Action<Stream> ResponseWriter { get; set; }

        /// <summary>
        ///     Collection of files to be sent with request
        /// </summary>
        public IList<HttpFile> Files { get; internal set; }

        /// <summary>
        ///     Whether or not HTTP 3xx response redirects should be automatically followed
        /// </summary>
        public bool FollowRedirects { get; set; }

        /// <summary>
        ///     Whether or not to use pipelined connections
        /// </summary>
        public bool Pipelined { get; set; }

        /// <summary>
        ///     X509CertificateCollection to be sent with request
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        ///     Maximum number of automatic redirects to follow if FollowRedirects is true
        /// </summary>
        public int? MaxRedirects { get; set; }

        /// <summary>
        ///     Determine whether or not the "default credentials" (e.g. the user account under which the current process is
        ///     running) ///     will be sent along to the server.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        ///     The ConnectionGroupName property enables you to associate a request with a connection group.
        /// </summary>
        public string ConnectionGroupName { get; set; }

        /// <summary>
        ///     Encoding for the request, UTF8 is the default
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     HTTP headers to be sent with request
        /// </summary>
        public IList<HttpHeader> Headers { get; internal set; }

        /// <summary>
        ///     HTTP parameters (QueryString or Form values) to be sent with request
        /// </summary>
        public IList<HttpParameter> Parameters { get; internal set; }

        /// <summary>
        ///     HTTP cookies to be sent with request
        /// </summary>
        public IList<HttpCookie> Cookies { get; internal set; }

        /// <summary>
        ///     Request body to be sent with request
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        ///     Content type of the request body.
        /// </summary>
        public string RequestContentType { get; set; }

        /// <summary>
        ///     An alternative to RequestBody, for when the caller already has the byte array.
        /// </summary>
        public byte[] RequestBodyBytes { get; set; }

        /// <summary>
        ///     URL to call for this request
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        ///     Explicit Host header value to use in requests independent from the request URI.
        ///     If null, default host value extracted from URI is used.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     List of Allowed Decompression Methods
        /// </summary>
        public IList<DecompressionMethods> AllowedDecompressionMethods { get; set; }

        /// <summary>
        ///     Flag to send authorisation header with the HttpWebRequest
        /// </summary>
        public bool PreAuthenticate { get; set; }

        /// <summary>
        ///     Flag to reuse same connection in the HttpWebRequest
        /// </summary>
        public bool UnsafeAuthenticatedConnectionSharing { get; set; }

        /// <summary>
        ///     Proxy info to be sent with request
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        ///     Caching policy for requests created with this wrapper.
        /// </summary>
        public RequestCachePolicy CachePolicy { get; set; }

        /// <summary>
        ///     Callback function for handling the validation of remote certificates.
        /// </summary>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }

        public Action<HttpWebRequest> WebRequestConfigurator { get; set; }

        /// <summary>
        ///     Creates an IHttp
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static IHttp Create() => new Http();

        [Obsolete("Overriding this method won't be possible in future version")]
        protected virtual HttpWebRequest CreateWebRequest(Uri url) => null;

        static HttpWebRequest CreateRequest(Uri uri) => (HttpWebRequest) WebRequest.Create(uri);

        static string GetMultipartFileHeader(HttpFile file)
            => $"--{FormBoundary}{LineBreak}Content-Disposition: form-data; name=\"{file.Name}\";" +
                $" filename=\"{file.FileName}\"{LineBreak}"                                        +
                $"Content-Type: {file.ContentType ?? "application/octet-stream"}{LineBreak}{LineBreak}";

        string GetMultipartFormData(HttpParameter param)
        {
            var format = param.Name == RequestContentType
                ? "--{0}{3}Content-Type: {4}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}"
                : "--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}";

            return string.Format(format, FormBoundary, param.Name, param.Value, LineBreak, param.ContentType);
        }

        static string GetMultipartFooter() => $"--{FormBoundary}--{LineBreak}";

        void PreparePostBody(WebRequest webRequest)
        {
            var needsContentType = string.IsNullOrEmpty(webRequest.ContentType);

            if (HasFiles || AlwaysMultipartFormData)
            {
                if (needsContentType)
                    webRequest.ContentType = GetMultipartFormContentType();
                else if (!webRequest.ContentType.Contains("boundary"))
                    webRequest.ContentType = webRequest.ContentType + "; boundary=" + FormBoundary;
            }
            else if (HasBody)
            {
                if (needsContentType)
                    webRequest.ContentType = RequestContentType;
            }
            else if (HasParameters)
            {
                if (needsContentType)
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                RequestBody = EncodeParameters();
            }

            string EncodeParameters() => string.Join("&", Parameters.Select(p => $"{Encode(p.Name)}={Encode(p.Value)}"));

            static string GetMultipartFormContentType() => $"multipart/form-data; boundary={FormBoundary}";
        }

        void WriteMultipartFormData(Stream requestStream)
        {
            foreach (var param in Parameters)
                requestStream.WriteString(GetMultipartFormData(param), Encoding);

            foreach (var file in Files)
            {
                // Add just the first part of this param, since we will write the file data directly to the Stream
                requestStream.WriteString(GetMultipartFileHeader(file), Encoding);

                // Write the file data directly to the Stream, rather than serializing it to a string.
                file.Writer(requestStream);
                requestStream.WriteString(LineBreak, Encoding);
            }

            requestStream.WriteString(GetMultipartFooter(), Encoding);
        }

        HttpResponse ExtractResponseData(HttpWebResponse webResponse)
        {
            var response = new HttpResponse
            {
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
                foreach (Cookie cookie in webResponse.Cookies)
                    response.Cookies.Add(
                        new HttpCookie
                        {
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

            foreach (var headerName in webResponse.Headers.AllKeys)
            {
                var headerValue = webResponse.Headers[headerName];

                response.Headers.Add(
                    new HttpHeader
                    {
                        Name  = headerName,
                        Value = headerValue
                    }
                );
            }

            var webResponseStream = webResponse.GetResponseStream();
            ProcessResponseStream();

            webResponse.Close();
            return response;

            void ProcessResponseStream()
            {
                if (AdvancedResponseWriter != null)
                {
                    AdvancedResponseWriter(webResponseStream, response);
                }
                else
                {
                    if (ResponseWriter == null)
                        response.RawBytes = webResponseStream.ReadAsBytes();
                    else
                        ResponseWriter(webResponseStream);
                }
            }
        }
    }
}