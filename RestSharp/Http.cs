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
using System.Text;
using RestSharp.Extensions;

#if WINDOWS_PHONE
using RestSharp.Compression.ZLib;
#endif

#if FRAMEWORK
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
#endif

namespace RestSharp
{
    /// <summary>
    /// HttpWebRequest wrapper
    /// </summary>
    public partial class Http : IHttp, IHttpFactory
    {
        private const string LINE_BREAK = "\r\n";

        ///<summary>
        /// Creates an IHttp
        ///</summary>
        ///<returns></returns>
        public IHttp Create()
        {
            return new Http();
        }

        /// <summary>
        /// True if this HTTP request has any HTTP parameters
        /// </summary>
        protected bool HasParameters
        {
            get { return this.Parameters.Any(); }
        }

        /// <summary>
        /// True if this HTTP request has any HTTP cookies
        /// </summary>
        protected bool HasCookies
        {
            get { return this.Cookies.Any(); }
        }

        /// <summary>
        /// True if a request body has been specified
        /// </summary>
        protected bool HasBody
        {
            get { return this.RequestBodyBytes != null || !string.IsNullOrEmpty(this.RequestBody); }
        }

        /// <summary>
        /// True if files have been set to be uploaded
        /// </summary>
        protected bool HasFiles
        {
            get { return this.Files.Any(); }
        }

        /// <summary>
        /// Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        public bool AlwaysMultipartFormData { get; set; }

        /// <summary>
        /// UserAgent to be sent with request
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Timeout in milliseconds to be used for the request
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// The number of milliseconds before the writing or reading times out.
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        /// System.Net.ICredentials to be sent with request
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// The System.Net.CookieContainer to be used for the request
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// The method to use to write the response instead of reading into RawBytes
        /// </summary>
        public Action<Stream> ResponseWriter { get; set; }

        /// <summary>
        /// Collection of files to be sent with request
        /// </summary>
        public IList<HttpFile> Files { get; private set; }

#if !SILVERLIGHT
        /// <summary>
        /// Whether or not HTTP 3xx response redirects should be automatically followed
        /// </summary>
        public bool FollowRedirects { get; set; }

        /// <summary>
        /// Whether or not to use pipelined connections
        /// </summary>
        public bool Pipelined { get; set; }
#endif

#if FRAMEWORK
        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        /// Maximum number of automatic redirects to follow if FollowRedirects is true
        /// </summary>
        public int? MaxRedirects { get; set; }

#endif

        /// <summary>
        /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is running)
        /// will be sent along to the server.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        private Encoding encoding = Encoding.UTF8;

        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        /// <summary>
        /// HTTP headers to be sent with request
        /// </summary>
        public IList<HttpHeader> Headers { get; private set; }

        /// <summary>
        /// HTTP parameters (QueryString or Form values) to be sent with request
        /// </summary>
        public IList<HttpParameter> Parameters { get; private set; }

        /// <summary>
        /// HTTP cookies to be sent with request
        /// </summary>
        public IList<HttpCookie> Cookies { get; private set; }

        /// <summary>
        /// Request body to be sent with request
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Content type of the request body.
        /// </summary>
        public string RequestContentType { get; set; }

        /// <summary>
        /// An alternative to RequestBody, for when the caller already has the byte array.
        /// </summary>
        public byte[] RequestBodyBytes { get; set; }

        /// <summary>
        /// URL to call for this request
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Flag to send authorisation header with the HttpWebRequest
        /// </summary>
        public bool PreAuthenticate { get; set; }

#if FRAMEWORK
        /// <summary>
        /// Proxy info to be sent with request
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Caching policy for requests created with this wrapper.
        /// </summary>
        public RequestCachePolicy CachePolicy { get; set; }
#endif
#if NET45
        /// <summary>
        /// Callback function for handling the validation of remote certificates.
        /// </summary>
        public RemoteCertificateValidationCallback  RemoteCertificateValidationCallback { get; set; }
#endif

        /// <summary>
        /// Default constructor
        /// </summary>
        public Http()
        {
            this.Headers = new List<HttpHeader>();
            this.Files = new List<HttpFile>();
            this.Parameters = new List<HttpParameter>();
            this.Cookies = new List<HttpCookie>();
            this.restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(
                StringComparer.OrdinalIgnoreCase);

            this.AddSharedHeaderActions();
            this.AddSyncHeaderActions();
        }

        partial void AddSyncHeaderActions();

#if SILVERLIGHT || WINDOWS_PHONE
        partial void AddAsyncHeaderActions();
#endif

        private void AddSharedHeaderActions()
        {
            this.restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);
            this.restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);
#if NET4
            restrictedHeaderActions.Add("Date", (r, v) =>
                                                {
                                                    DateTime parsed;

                                                    if (DateTime.TryParse(v, out parsed))
                                                    {
                                                        r.Date = parsed;
                                                    }
                                                });

            restrictedHeaderActions.Add("Host", (r, v) => r.Host = v);
#else
            this.restrictedHeaderActions.Add("Date", (r, v) => { /* Set by system */ });
            this.restrictedHeaderActions.Add("Host", (r, v) => { /* Set by system */ });
#endif

#if FRAMEWORK
            this.restrictedHeaderActions.Add("Range", AddRange);
#endif
        }

        private const string FORM_BOUNDARY = "-----------------------------28947758029299";

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
            string format = param.Name == this.RequestContentType
                ? "--{0}{3}Content-Type: {4}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}"
                : "--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}";

            return string.Format(format, FORM_BOUNDARY, param.Name, param.Value, LINE_BREAK, param.ContentType);
        }

        private static string GetMultipartFooter()
        {
            return string.Format("--{0}--{1}", FORM_BOUNDARY, LINE_BREAK);
        }

        private readonly IDictionary<string, Action<HttpWebRequest, string>> restrictedHeaderActions;

        // handle restricted headers the .NET way - thanks @dimebrain!
        // http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
        private void AppendHeaders(HttpWebRequest webRequest)
        {
            foreach (HttpHeader header in this.Headers)
            {
                if (this.restrictedHeaderActions.ContainsKey(header.Name))
                {
                    this.restrictedHeaderActions[header.Name].Invoke(webRequest, header.Value);
                }
                else
                {
#if FRAMEWORK
                    webRequest.Headers.Add(header.Name, header.Value);
#else
                    webRequest.Headers[header.Name] = header.Value;
#endif
                }
            }
        }

        private void AppendCookies(HttpWebRequest webRequest)
        {
            webRequest.CookieContainer = this.CookieContainer ?? new CookieContainer();

            foreach (HttpCookie httpCookie in this.Cookies)
            {
#if FRAMEWORK
                Cookie cookie = new Cookie
                                {
                                    Name = httpCookie.Name,
                                    Value = httpCookie.Value,
                                    Domain = webRequest.RequestUri.Host
                                };

                webRequest.CookieContainer.Add(cookie);
#else
                Cookie cookie = new Cookie
                             {
                                 Name = httpCookie.Name,
                                 Value = httpCookie.Value
                             };
                Uri uri = webRequest.RequestUri;

                webRequest.CookieContainer.Add(new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host)), cookie);
#endif
            }
        }

        private string EncodeParameters()
        {
            StringBuilder querystring = new StringBuilder();

            foreach (HttpParameter p in this.Parameters)
            {
                if (querystring.Length > 1)
                {
                    querystring.Append("&");
                }

                querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), p.Value.UrlEncode());
            }

            return querystring.ToString();
        }

        private void PreparePostBody(HttpWebRequest webRequest)
        {
            if (this.HasFiles || this.AlwaysMultipartFormData)
            {
                webRequest.ContentType = GetMultipartFormContentType();
            }
            else if (this.HasParameters)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
                this.RequestBody = this.EncodeParameters();
            }
            else if (this.HasBody)
            {
                webRequest.ContentType = this.RequestContentType;
            }
        }

        private void WriteStringTo(Stream stream, string toWrite)
        {
            byte[] bytes = this.Encoding.GetBytes(toWrite);

            stream.Write(bytes, 0, bytes.Length);
        }

        private void WriteMultipartFormData(Stream requestStream)
        {
            foreach (HttpParameter param in this.Parameters)
            {
                this.WriteStringTo(requestStream, this.GetMultipartFormData(param));
            }

            foreach (HttpFile file in this.Files)
            {
                // Add just the first part of this param, since we will write the file data directly to the Stream
                this.WriteStringTo(requestStream, GetMultipartFileHeader(file));

                // Write the file data directly to the Stream, rather than serializing it to a string.
                file.Writer(requestStream);
                this.WriteStringTo(requestStream, LINE_BREAK);
            }

            this.WriteStringTo(requestStream, GetMultipartFooter());
        }

        private void ExtractResponseData(HttpResponse response, HttpWebResponse webResponse)
        {
            using (webResponse)
            {
#if FRAMEWORK
                response.ContentEncoding = webResponse.ContentEncoding;
                response.Server = webResponse.Server;
                response.ProtocolVersion = webResponse.ProtocolVersion;
#endif
                response.ContentType = webResponse.ContentType;
                response.ContentLength = webResponse.ContentLength;

                Stream webResponseStream = webResponse.GetResponseStream();

#if WINDOWS_PHONE
                if (string.Equals(webResponse.Headers[HttpRequestHeader.ContentEncoding], "gzip", StringComparison.OrdinalIgnoreCase))
                {
                    GZipStream gzStream = new GZipStream(webResponseStream);

                    ProcessResponseStream(gzStream, response);
                }
                else
                {
                    ProcessResponseStream(webResponseStream, response);
                }
#else
                this.ProcessResponseStream(webResponseStream, response);
#endif

                response.StatusCode = webResponse.StatusCode;
                response.StatusDescription = webResponse.StatusDescription;
                response.ResponseUri = webResponse.ResponseUri;
                response.ResponseStatus = ResponseStatus.Completed;

                if (webResponse.Cookies != null)
                {
                    foreach (Cookie cookie in webResponse.Cookies)
                    {
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
                    }
                }

                foreach (string headerName in webResponse.Headers.AllKeys)
                {
                    string headerValue = webResponse.Headers[headerName];

                    response.Headers.Add(new HttpHeader
                                         {
                                             Name = headerName,
                                             Value = headerValue
                                         });
                }
#if !WINDOWS_UWP
                webResponse.Close();
#else
                webResponse.Dispose();
#endif
            }
        }

        private void ProcessResponseStream(Stream webResponseStream, HttpResponse response)
        {
            if (this.ResponseWriter == null)
            {
                response.RawBytes = webResponseStream.ReadAsBytes();
            }
            else
            {
                this.ResponseWriter(webResponseStream);
            }
        }

#if FRAMEWORK
        private static void AddRange(HttpWebRequest r, string range)
        {
            Match m = Regex.Match(range, "(\\w+)=(\\d+)-(\\d+)$");

            if (!m.Success)
            {
                return;
            }

            string rangeSpecifier = m.Groups[1].Value;
            int from = Convert.ToInt32(m.Groups[2].Value);
            int to = Convert.ToInt32(m.Groups[3].Value);

            r.AddRange(rangeSpecifier, from, to);
        }
#endif
    }
}
