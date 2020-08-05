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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JetBrains.Annotations;

namespace RestSharp
{
    [PublicAPI]
    public interface IHttp
    {
        /// <summary>
        /// The delegate to use to write the response instead of reading into RawBytes
        /// </summary>
        Action<Stream> ResponseWriter { get; set; }

        /// <summary>
        /// The delegate to use to write the response instead of reading into RawBytes
        /// Here you can also check the request details
        /// </summary>
        Action<Stream, IHttpResponse> AdvancedResponseWriter { get; set; }

        /// <summary>
        /// The <see cref="System.Net.CookieContainer"/> to be used for the request
        /// </summary>
        CookieContainer? CookieContainer { get; set; }

        /// <summary>
        /// <see cref="System.Net.ICredentials"/> to be sent with request
        /// </summary>
        ICredentials? Credentials { get; set; }

        /// <summary>
        /// Enable or disable automatic gzip/deflate decompression
        /// </summary>
        bool AutomaticDecompression { get; set; }

        /// <summary>
        /// Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        bool AlwaysMultipartFormData { get; set; }

        /// <summary>
        /// </summary>
        string UserAgent { get; set; }

        /// <summary>
        /// Timeout in milliseconds to be used for the request
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// The number of milliseconds before the writing or reading times out.
        /// </summary>
        int ReadWriteTimeout { get; set; }

        /// <summary>
        /// Whether or not HTTP 3xx response redirects should be automatically followed
        /// </summary>
        bool FollowRedirects { get; set; }

        /// <summary>
        /// Whether or not to use pipelined connections
        /// </summary>
        bool Pipelined { get; set; }

        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        X509CertificateCollection? ClientCertificates { get; set; }

        /// <summary>
        /// Maximum number of automatic redirects to follow if FollowRedirects is true
        /// </summary>
        int? MaxRedirects { get; set; }

        /// <summary>
        /// Determine whether or not the "default credentials" (e.g. the user account under which the
        /// current process is running) will be sent along to the server.
        /// </summary>
        bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Encoding for the request, UTF8 is the default
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// HTTP headers to be sent with request
        /// </summary>
        IList<HttpHeader> Headers { get; }

        /// <summary>
        /// HTTP parameters (QueryString or Form values) to be sent with request
        /// </summary>
        IList<HttpParameter> Parameters { get; }

        /// <summary>
        /// Collection of files to be sent with request
        /// </summary>
        IList<HttpFile> Files { get; }

        /// <summary>
        /// HTTP cookies to be sent with request
        /// </summary>
        IList<HttpCookie> Cookies { get; }

        /// <summary>
        /// Request body to be sent with request
        /// </summary>
        string RequestBody { get; set; }

        /// <summary>
        /// Content type of the request body.
        /// </summary>
        string RequestContentType { get; set; }

        /// <summary>
        /// Flag to send authorisation header with the HttpWebRequest
        /// </summary>
        bool PreAuthenticate { get; set; }

        /// <summary>
        /// Flag to reuse same connection in the HttpWebRequest
        /// </summary>
        bool UnsafeAuthenticatedConnectionSharing { get; set; }

        /// <summary>
        /// Caching policy for requests created with this wrapper.
        /// </summary>
        RequestCachePolicy? CachePolicy { get; set; }

        /// <summary>
        /// The ConnectionGroupName property enables you to associate a request with a connection group.
        /// </summary>
        string ConnectionGroupName { get; set; }

        /// <summary>
        /// An alternative to RequestBody, for when the caller already has the byte array.
        /// </summary>
        byte[] RequestBodyBytes { get; set; }

        /// <summary>
        /// URL to call for this request
        /// </summary>
        Uri Url { get; set; }

        /// <summary>
        /// Explicit Host header value to use in requests independent from the request URI.
        /// If null, default host value extracted from URI is used.
        /// </summary>
        string? Host { get; set; }

        /// <summary>
        /// Boundary that will be used for multipart/form-data requests
        /// </summary>
        string FormBoundary { get; }

        /// <summary>
        /// List of allowed decompression methods
        /// </summary>
        IList<DecompressionMethods> AllowedDecompressionMethods { get; set; }

        /// <summary>
        /// Proxy info to be sent with request
        /// </summary>
        IWebProxy? Proxy { get; set; }

        RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

        Action<HttpWebRequest>? WebRequestConfigurator { get; set; }

        [Obsolete]
        HttpWebRequest DeleteAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest GetAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest HeadAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest OptionsAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest PostAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest PutAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest PatchAsync(Action<HttpResponse> action);

        [Obsolete]
        HttpWebRequest MergeAsync(Action<HttpResponse> action);

        /// <summary>
        /// Execute an async POST-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        HttpWebRequest? AsPostAsync(Action<HttpResponse> action, string httpMethod);

        /// <summary>
        /// Execute an async GET-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        HttpWebRequest? AsGetAsync(Action<HttpResponse> action, string httpMethod);

        HttpResponse Delete();

        HttpResponse Get();

        HttpResponse Head();

        HttpResponse Options();

        HttpResponse Post();

        HttpResponse Put();

        HttpResponse Patch();

        HttpResponse Merge();

        HttpResponse AsPost(string httpMethod);

        HttpResponse AsGet(string httpMethod);
    }
}