#region License

//   Copyright 2010 John Sheehan
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
        private const string LINE_BREAK = "\r\n";

        private const string FORM_BOUNDARY = "-----------------------------28947758029299";

        private readonly IDictionary<string, Action<HttpWebRequest, string>> restrictedHeaderActions;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public Http()
        {
            Headers = new List<HttpHeader>();
            Files = new List<HttpFile>();
            Parameters = new List<HttpParameter>();
            Cookies = new List<HttpCookie>();
            restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(
                StringComparer.OrdinalIgnoreCase);

            AddSharedHeaderActions();
            AddSyncHeaderActions();
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

        /// <summary>
        /// Enable or disable automatic gzip/deflate decompression
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
        ///     The method to use to write the response instead of reading into RawBytes
        /// </summary>
        public Action<Stream> ResponseWriter { get; set; }

        /// <summary>
        ///     Collection of files to be sent with request
        /// </summary>
        public IList<HttpFile> Files { get; }

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
        ///	    The ConnectionGroupName property enables you to associate a request with a connection group. 
        /// </summary>
        public string ConnectionGroupName { get; set; }

        /// <summary>
        ///     Encoding for the request, UTF8 is the default
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     HTTP headers to be sent with request
        /// </summary>
        public IList<HttpHeader> Headers { get; }

        /// <summary>
        ///     HTTP parameters (QueryString or Form values) to be sent with request
        /// </summary>
        public IList<HttpParameter> Parameters { get; }

        /// <summary>
        ///     HTTP cookies to be sent with request
        /// </summary>
        public IList<HttpCookie> Cookies { get; }

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
        /// Explicit Host header value to use in requests independent from the request URI.
        /// If null, default host value extracted from URI is used.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// List of Allowed Decompression Methods
        /// </summary>
        public IList<DecompressionMethods> AllowedDecompressionMethods { get; set; }

        /// <summary>
        ///     Flag to send authorisation header with the HttpWebRequest
        /// </summary>
        public bool PreAuthenticate { get; set; }

        /// <summary>
        /// Flag to reuse same connection in the HttpWebRequest
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

        /// <summary>
        ///     Creates an IHttp
        /// </summary>
        /// <returns></returns>
        public static IHttp Create() => new Http();

        protected virtual HttpWebRequest CreateWebRequest(Uri url) => (HttpWebRequest) WebRequest.Create(url);

        public Action<HttpWebRequest> WebRequestConfigurator { get; set; }
        
        partial void AddSyncHeaderActions();

        private void AddSharedHeaderActions()
        {
            restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);
            restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);
            restrictedHeaderActions.Add("Date", (r, v) =>
            {
                if (DateTime.TryParse(v, out var parsed))
                    r.Date = parsed;
            });

            restrictedHeaderActions.Add("Host", (r, v) => r.Host = v);

            restrictedHeaderActions.Add("Range", AddRange);
        }

        private static string GetMultipartFormContentType()
        {
            return string.Format("multipart/form-data; boundary={0}", FORM_BOUNDARY);
        }

        private static string GetMultipartFileHeader(HttpFile file)
        {
            return string.Format(
                "--{0}{4}Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"{4}Content-Type: {3}{4}{4}",
                FORM_BOUNDARY, file.Name, file.FileName, file.ContentType ?? "application/octet-stream", LINE_BREAK);
        }

        private string GetMultipartFormData(HttpParameter param)
        {
            var format = param.Name == RequestContentType
                ? "--{0}{3}Content-Type: {4}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}"
                : "--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}";

            return string.Format(format, FORM_BOUNDARY, param.Name, param.Value, LINE_BREAK, param.ContentType);
        }

        private static string GetMultipartFooter()
        {
            return string.Format("--{0}--{1}", FORM_BOUNDARY, LINE_BREAK);
        }

        // handle restricted headers the .NET way - thanks @dimebrain!
        // http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
        private void AppendHeaders(HttpWebRequest webRequest)
        {
            foreach (var header in Headers)
                if (restrictedHeaderActions.ContainsKey(header.Name))
                    restrictedHeaderActions[header.Name].Invoke(webRequest, header.Value);
                else
                    webRequest.Headers.Add(header.Name, header.Value);
        }

        private void AppendCookies(HttpWebRequest webRequest)
        {
            webRequest.CookieContainer = CookieContainer ?? new CookieContainer();

            foreach (var httpCookie in Cookies)
            {
                var cookie = new Cookie
                {
                    Name = httpCookie.Name,
                    Value = httpCookie.Value,
                    Domain = webRequest.RequestUri.Host
                };

                webRequest.CookieContainer.Add(cookie);
            }
        }

        private string EncodeParameters()
        {
            var querystring = new StringBuilder();

            foreach (var p in Parameters)
            {
                if (querystring.Length > 1)
                    querystring.Append("&");

                querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), p.Value.UrlEncode());
            }

            return querystring.ToString();
        }

        private void PreparePostBody(HttpWebRequest webRequest)
        {
            bool needsContentType = String.IsNullOrEmpty(webRequest.ContentType);

            if (HasFiles || AlwaysMultipartFormData)
            {
                if (needsContentType)
                    webRequest.ContentType = GetMultipartFormContentType();
            }
            else if (HasParameters)
            {
                if (needsContentType)
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                RequestBody = EncodeParameters();
            }
            else if (HasBody)
            {
                if (needsContentType)
                    webRequest.ContentType = RequestContentType;
            }
        }

        private void WriteStringTo(Stream stream, string toWrite)
        {
            var bytes = Encoding.GetBytes(toWrite);

            stream.Write(bytes, 0, bytes.Length);
        }

        private void WriteMultipartFormData(Stream requestStream)
        {
            foreach (var param in Parameters)
                WriteStringTo(requestStream, GetMultipartFormData(param));

            foreach (var file in Files)
            {
                // Add just the first part of this param, since we will write the file data directly to the Stream
                WriteStringTo(requestStream, GetMultipartFileHeader(file));

                // Write the file data directly to the Stream, rather than serializing it to a string.
                file.Writer(requestStream);
                WriteStringTo(requestStream, LINE_BREAK);
            }

            WriteStringTo(requestStream, GetMultipartFooter());
        }

        private void ExtractResponseData(HttpResponse response, HttpWebResponse webResponse)
        {
            using (webResponse)
            {
                response.ContentEncoding = webResponse.ContentEncoding;
                response.Server = webResponse.Server;
                response.ProtocolVersion = webResponse.ProtocolVersion;
                response.ContentType = webResponse.ContentType;
                response.ContentLength = webResponse.ContentLength;

                var webResponseStream = webResponse.GetResponseStream();

                ProcessResponseStream(webResponseStream, response);

                response.StatusCode = webResponse.StatusCode;
                response.StatusDescription = webResponse.StatusDescription;
                response.ResponseUri = webResponse.ResponseUri;
                response.ResponseStatus = ResponseStatus.Completed;

                if (webResponse.Cookies != null)
                    foreach (Cookie cookie in webResponse.Cookies)
                        response.Cookies.Add(new HttpCookie
                        {
                            Comment = cookie.Comment,
                            CommentUri = cookie.CommentUri,
                            Discard = cookie.Discard,
                            Domain = cookie.Domain,
                            Expired = cookie.Expired,
                            Expires = cookie.Expires,
                            HttpOnly = cookie.HttpOnly,
                            Name = cookie.Name,
                            Path = cookie.Path,
                            Port = cookie.Port,
                            Secure = cookie.Secure,
                            TimeStamp = cookie.TimeStamp,
                            Value = cookie.Value,
                            Version = cookie.Version
                        });

                foreach (var headerName in webResponse.Headers.AllKeys)
                {
                    var headerValue = webResponse.Headers[headerName];

                    response.Headers.Add(new HttpHeader
                    {
                        Name = headerName,
                        Value = headerValue
                    });
                }

                webResponse.Close();
            }
        }

        private void ProcessResponseStream(Stream webResponseStream, HttpResponse response)
        {
            if (ResponseWriter == null)
                response.RawBytes = webResponseStream.ReadAsBytes();
            else
                ResponseWriter(webResponseStream);
        }

        private static readonly Regex AddRangeRegex = new Regex("(\\w+)=(\\d+)-(\\d+)$");

        private static void AddRange(HttpWebRequest r, string range)
        {
            var m = AddRangeRegex.Match(range);

            if (!m.Success)
                return;

            string rangeSpecifier = m.Groups[1].Value;
            long from = Convert.ToInt64(m.Groups[2].Value);
            long to = Convert.ToInt64(m.Groups[3].Value);

            r.AddRange(rangeSpecifier, from, to);
        }
    }
}