using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RestSharp
{
    public class HttpRequest : IHttpRequest
    {
        public HttpRequest()
        {
            Headers = new List<HttpHeader>();
            Files = new List<HttpFile>();
            Parameters = new List<KeyValuePair<string, string>>();
            Cookies = new List<HttpCookie>();
        }

        #region Public Properties

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

        /// <summary>
        /// Whether or not HTTP 3xx response redirects should be automatically followed
        /// </summary>
        public bool FollowRedirects { get; set; }

        /// <summary>
        /// Maximum number of automatic redirects to follow
        /// </summary>
        public int? MaxAutomaticRedirects { get; set; }

        /// <summary>
        /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is running)
        /// will be sent along to the server.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// HTTP headers to be sent with request
        /// </summary>
        public IList<HttpHeader> Headers { get; private set; }

        /// <summary>
        /// HTTP parameters (QueryString or Form values) to be sent with request
        /// </summary>
        public IList<KeyValuePair<string, string>> Parameters { get; private set; }

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
        /// Proxy info to be sent with request
        /// </summary>
        public IWebProxy Proxy { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// True if this HTTP request has any HTTP parameters
        /// </summary>
        public bool HasParameters
        {
            get
            {
                return Parameters.Any();
            }
        }

        /// <summary>
        /// True if this HTTP request has any HTTP cookies
        /// </summary>
        public bool HasCookies
        {
            get
            {
                return Cookies.Any();
            }
        }

        /// <summary>
        /// True if a request body has been specified
        /// </summary>
        public bool HasBody
        {
            get
            {
                return RequestBodyBytes != null || !string.IsNullOrEmpty(RequestBody);
            }
        }

        /// <summary>
        /// True if files have been set to be uploaded
        /// </summary>
        public bool HasFiles
        {
            get
            {
                return Files.Any();
            }
        }

        #endregion
    }
}
