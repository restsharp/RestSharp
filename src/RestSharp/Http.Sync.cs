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
using System.Globalization;
using System.Net;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    ///     HttpWebRequest wrapper (sync methods)
    /// </summary>
    public partial class Http
    {
        /// <summary>
        ///     Execute a POST request
        /// </summary>
        public HttpResponse Post() => PostPutInternal("POST");

        /// <summary>
        ///     Execute a PUT request
        /// </summary>
        public HttpResponse Put() => PostPutInternal("PUT");

        /// <summary>
        ///     Execute a GET request
        /// </summary>
        public HttpResponse Get() => GetStyleMethodInternal("GET");

        /// <summary>
        ///     Execute a HEAD request
        /// </summary>
        public HttpResponse Head() => GetStyleMethodInternal("HEAD");

        /// <summary>
        ///     Execute an OPTIONS request
        /// </summary>
        public HttpResponse Options() => GetStyleMethodInternal("OPTIONS");

        /// <summary>
        ///     Execute a DELETE request
        /// </summary>
        public HttpResponse Delete() => GetStyleMethodInternal("DELETE");

        /// <summary>
        ///     Execute a PATCH request
        /// </summary>
        public HttpResponse Patch() => PostPutInternal("PATCH");

        /// <summary>
        ///     Execute a MERGE request
        /// </summary>
        public HttpResponse Merge() => PostPutInternal("MERGE");

        /// <summary>
        ///     Execute a GET-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsGet(string httpMethod) => GetStyleMethodInternal(httpMethod.ToUpperInvariant());

        /// <summary>
        ///     Execute a POST-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsPost(string httpMethod) => PostPutInternal(httpMethod.ToUpperInvariant());

        HttpResponse GetStyleMethodInternal(string method)
            => ExecuteRequest(
                method, r =>
                {
                    if (!HasBody) return;
                    
                    if (!CanGetWithBody())
                        throw new NotSupportedException($"Http verb {method} does not support body");

                    r.ContentType = RequestContentType;
                    WriteRequestBody(r);

                    bool CanGetWithBody() => method == "DELETE" || method == "OPTIONS";
                }
            );

        HttpResponse PostPutInternal(string method)
            => ExecuteRequest(
                method, r =>
                {
                    PreparePostBody(r);
                    WriteRequestBody(r);
                }
            );

        HttpResponse ExecuteRequest(string httpMethod, Action<HttpWebRequest> prepareRequest)
        {
            var webRequest = ConfigureWebRequest(httpMethod, Url);

            prepareRequest(webRequest);

            try
            {
                using var webResponse = GetRawResponse(webRequest);

                return ExtractResponseData(webResponse);
            }
            catch (Exception ex)
            {
                return ExtractErrorResponse(ex);
            }

            static HttpResponse ExtractErrorResponse(Exception ex)
            {
                var response = new HttpResponse {ErrorMessage = ex.Message};

                if (ex is WebException webException && webException.Status == WebExceptionStatus.Timeout)
                {
                    response.ResponseStatus = ResponseStatus.TimedOut;
                    response.ErrorException = webException;
                }
                else
                {
                    response.ErrorException = ex;
                    response.ResponseStatus = ResponseStatus.Error;
                }

                return response;
            }

            static HttpWebResponse GetRawResponse(WebRequest request)
            {
                try
                {
                    return (HttpWebResponse) request.GetResponse();
                }
                catch (WebException ex)
                {
                    // Check to see if this is an HTTP error or a transport error.
                    // In cases where an HTTP error occurs ( status code >= 400 )
                    // return the underlying HTTP response, otherwise assume a
                    // transport exception (ex: connection timeout) and
                    // rethrow the exception

                    if (ex.Response is HttpWebResponse response)
                        return response;

                    throw;
                }
            }
        }

        void WriteRequestBody(WebRequest webRequest)
        {
            if (HasBody || HasFiles || AlwaysMultipartFormData)
                webRequest.ContentLength = CalculateContentLength();

            using var requestStream = webRequest.GetRequestStream();

            if (HasFiles || AlwaysMultipartFormData)
                WriteMultipartFormData(requestStream);
            else if (RequestBodyBytes != null)
                requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
            else if (RequestBody != null)
                requestStream.WriteString(RequestBody, Encoding);
        }

        [Obsolete("Use the WebRequestConfigurator delegate instead of overriding this method")]
        protected virtual HttpWebRequest ConfigureWebRequest(string method, Uri url)
        {
            var webRequest = CreateWebRequest(url) ?? CreateRequest(url);

            webRequest.UseDefaultCredentials = UseDefaultCredentials;

            webRequest.PreAuthenticate                      = PreAuthenticate;
            webRequest.Pipelined                            = Pipelined;
            webRequest.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing;
#if NETSTANDARD2_0
            webRequest.Proxy = null;
#endif
            try
            {
                webRequest.ServicePoint.Expect100Continue = false;
            }
            catch (PlatformNotSupportedException)
            {
                // Avoid to crash in UWP apps
            }

            AppendHeaders();
            AppendCookies();

            if (Host != null) webRequest.Host = Host;

            webRequest.Method = method;

            // make sure Content-Length header is always sent since default is -1
            if (!HasFiles && !AlwaysMultipartFormData && method != "GET")
                webRequest.ContentLength = 0;

            if (Credentials != null)
                webRequest.Credentials = Credentials;

            if (UserAgent.HasValue())
                webRequest.UserAgent = UserAgent;

            if (ClientCertificates != null)
                webRequest.ClientCertificates.AddRange(ClientCertificates);

            AllowedDecompressionMethods.ForEach(x => webRequest.AutomaticDecompression |= x);

            if (AutomaticDecompression)
                webRequest.AutomaticDecompression =
                    DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

            if (Timeout != 0)
                webRequest.Timeout = Timeout;

            if (ReadWriteTimeout != 0)
                webRequest.ReadWriteTimeout = ReadWriteTimeout;

            webRequest.Proxy = Proxy;

            if (CachePolicy != null)
                webRequest.CachePolicy = CachePolicy;

            webRequest.AllowAutoRedirect = FollowRedirects;

            if (FollowRedirects && MaxRedirects.HasValue)
                webRequest.MaximumAutomaticRedirections = MaxRedirects.Value;

            webRequest.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

            webRequest.ConnectionGroupName = ConnectionGroupName;

            WebRequestConfigurator?.Invoke(webRequest);

            return webRequest;

            // handle restricted headers the .NET way - thanks @dimebrain!
            // http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
            void AppendHeaders()
            {
                foreach (var header in Headers)
                {
                    if (_restrictedHeaderActions.TryGetValue(header.Name, out var restrictedHeaderAction))
                        restrictedHeaderAction.Invoke(webRequest, header.Value);
                    else
                        webRequest.Headers.Add(header.Name, header.Value);
                }
            }

            void AppendCookies()
            {
                webRequest.CookieContainer = CookieContainer ?? new CookieContainer();

                foreach (var httpCookie in Cookies)
                {
                    var cookie = new Cookie
                    {
                        Name   = httpCookie.Name,
                        Value  = httpCookie.Value,
                        Domain = webRequest.RequestUri.Host
                    };

                    webRequest.CookieContainer.Add(cookie);
                }
            }
        }
    }
}