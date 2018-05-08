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
using System.Globalization;
using System.Net;
using RestSharp.Extensions;
using RestSharp.Authenticators.OAuth.Extensions;

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

        private HttpResponse GetStyleMethodInternal(string method)
        {
            var webRequest = ConfigureWebRequest(method, Url);

            if (HasBody && (method == "DELETE" || method == "OPTIONS"))
            {
                webRequest.ContentType = RequestContentType;
                WriteRequestBody(webRequest);
            }

            return GetResponse(webRequest);
        }

        private HttpResponse PostPutInternal(string method)
        {
            var webRequest = ConfigureWebRequest(method, Url);

            PreparePostBody(webRequest);

            WriteRequestBody(webRequest);
            return GetResponse(webRequest);
        }

        partial void AddSyncHeaderActions()
        {
            restrictedHeaderActions.Add("Connection", (r, v) => { r.KeepAlive = v.ToLower().Contains("keep-alive"); });
            restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
            restrictedHeaderActions.Add("Expect", (r, v) => r.Expect = v);
            restrictedHeaderActions.Add("If-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v, CultureInfo.InvariantCulture));
            restrictedHeaderActions.Add("Referer", (r, v) => r.Referer = v);
            restrictedHeaderActions.Add("Transfer-Encoding", (r, v) =>
            {
                r.TransferEncoding = v;
                r.SendChunked = true;
            });
            restrictedHeaderActions.Add("User-Agent", (r, v) => r.UserAgent = v);
        }

        private static void ExtractErrorResponse(IHttpResponse httpResponse, Exception ex)
        {
            if (ex is WebException webException && webException.Status == WebExceptionStatus.Timeout)
            {
                httpResponse.ResponseStatus = ResponseStatus.TimedOut;
                httpResponse.ErrorMessage = ex.Message;
                httpResponse.ErrorException = webException;
            }
            else
            {
                httpResponse.ErrorMessage = ex.Message;
                httpResponse.ErrorException = ex;
                httpResponse.ResponseStatus = ResponseStatus.Error;
            }
        }

        private HttpResponse GetResponse(HttpWebRequest request)
        {
            var response = new HttpResponse {ResponseStatus = ResponseStatus.None};

            try
            {
                var webResponse = GetRawResponse(request);

                ExtractResponseData(response, webResponse);
            }
            catch (Exception ex)
            {
                ExtractErrorResponse(response, ex);
            }

            return response;
        }

        private static HttpWebResponse GetRawResponse(HttpWebRequest request)
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

        private void WriteRequestBody(HttpWebRequest webRequest)
        {
            if (HasBody || HasFiles || AlwaysMultipartFormData)
                webRequest.ContentLength = CalculateContentLength();

            using (var requestStream = webRequest.GetRequestStream())
            {
                if (HasFiles || AlwaysMultipartFormData)
                    WriteMultipartFormData(requestStream);
                else if (RequestBodyBytes != null)
                    requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
                else if (RequestBody != null)
                    WriteStringTo(requestStream, RequestBody);
            }
        }

        protected virtual HttpWebRequest ConfigureWebRequest(string method, Uri url)
        {
            var webRequest = CreateWebRequest(url);

            webRequest.UseDefaultCredentials = UseDefaultCredentials;

            webRequest.PreAuthenticate = PreAuthenticate;
            webRequest.Pipelined = Pipelined;
            webRequest.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing;
#if NETSTANDARD2_0
            webRequest.Proxy = null;
#endif
            webRequest.ServicePoint.Expect100Continue = false;
            
            AppendHeaders(webRequest);
            AppendCookies(webRequest);

            if (Host != null) webRequest.Host = Host;

            webRequest.Method = method;

            // make sure Content-Length header is always sent since default is -1
            if (!HasFiles && !AlwaysMultipartFormData)
                webRequest.ContentLength = 0;

            if (Credentials != null)
                webRequest.Credentials = Credentials;

            if (UserAgent.HasValue())
                webRequest.UserAgent = UserAgent;

            if (ClientCertificates != null)
                webRequest.ClientCertificates.AddRange(ClientCertificates);
            
            AllowedDecompressionMethods.ForEach(x => { webRequest.AutomaticDecompression |= x; });            

            if (AutomaticDecompression)
            {
                webRequest.AutomaticDecompression =
                    DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;
            }

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
        }
    }
}