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
using System.IO;
using System.Net;
using RestSharp.Extensions;

#if !WINDOWS_PHONE
using System.Linq;
#endif

#if SILVERLIGHT
using System.Net.Browser;
#endif

#if !WINDOWS_PHONE && !SILVERLIGHT
using System.Threading;
#endif

namespace RestSharp
{
    /// <summary>
    /// HttpWebRequest wrapper (async methods)
    /// </summary>
    public partial class Http
    {
        private TimeOutState timeoutState;

        public HttpWebRequest DeleteAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("DELETE", action);
        }

        public HttpWebRequest GetAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("GET", action);
        }

        public HttpWebRequest HeadAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("HEAD", action);
        }

        public HttpWebRequest OptionsAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("OPTIONS", action);
        }

        public HttpWebRequest PostAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("POST", action);
        }

        public HttpWebRequest PutAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("PUT", action);
        }

        public HttpWebRequest PatchAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("PATCH", action);
        }

        public HttpWebRequest MergeAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("MERGE", action);
        }

        /// <summary>
        /// Execute an async POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
        {
            return PutPostInternalAsync(httpMethod.ToUpperInvariant(), action);
        }

        /// <summary>
        /// Execute an async GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpWebRequest AsGetAsync(Action<HttpResponse> action, string httpMethod)
        {
            return GetStyleMethodInternalAsync(httpMethod.ToUpperInvariant(), action);
        }

        private HttpWebRequest GetStyleMethodInternalAsync(string method, Action<HttpResponse> callback)
        {
            HttpWebRequest webRequest = null;

            try
            {
                Uri url = Url;

                webRequest = ConfigureAsyncWebRequest(method, url);

                if (HasBody && (method == "DELETE" || method == "OPTIONS"))
                {
                    webRequest.ContentType = RequestContentType;
                    WriteRequestBodyAsync(webRequest, callback);
                }
                else
                {
                    this.timeoutState = new TimeOutState { Request = webRequest };

                    IAsyncResult asyncResult = webRequest.BeginGetResponse(result => ResponseCallback(result, callback), webRequest);

                    SetTimeout(asyncResult, this.timeoutState);
                }
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }

            return webRequest;
        }

        private HttpResponse CreateErrorResponse(Exception ex)
        {
            HttpResponse response = new HttpResponse();
            WebException webException = ex as WebException;

            if (webException != null && webException.Status == WebExceptionStatus.RequestCanceled)
            {
                response.ResponseStatus = this.timeoutState.TimedOut ? ResponseStatus.TimedOut : ResponseStatus.Aborted;

                return response;
            }

            response.ErrorMessage = ex.Message;
            response.ErrorException = ex;
            response.ResponseStatus = ResponseStatus.Error;

            return response;
        }

        private HttpWebRequest PutPostInternalAsync(string method, Action<HttpResponse> callback)
        {
            HttpWebRequest webRequest = null;

            try
            {
                webRequest = ConfigureAsyncWebRequest(method, Url);
                PreparePostBody(webRequest);
                WriteRequestBodyAsync(webRequest, callback);
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }

            return webRequest;
        }

        private void WriteRequestBodyAsync(HttpWebRequest webRequest, Action<HttpResponse> callback)
        {
            IAsyncResult asyncResult;
            this.timeoutState = new TimeOutState { Request = webRequest };

            if (HasBody || HasFiles || AlwaysMultipartFormData)
            {
#if !WINDOWS_PHONE
                webRequest.ContentLength = CalculateContentLength();
#endif
                asyncResult = webRequest.BeginGetRequestStream(result => RequestStreamCallback(result, callback), webRequest);
            }
            else
            {
                asyncResult = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
            }

            SetTimeout(asyncResult, this.timeoutState);
        }

#if !WINDOWS_PHONE
        private long CalculateContentLength()
        {
            if (RequestBodyBytes != null)
                return RequestBodyBytes.Length;

            if (!HasFiles && !AlwaysMultipartFormData)
            {
                return encoding.GetByteCount(RequestBody);
            }

            // calculate length for multipart form
            long length = 0;

            foreach (HttpFile file in Files)
            {
                length += this.Encoding.GetByteCount(GetMultipartFileHeader(file));
                length += file.ContentLength;
                length += this.Encoding.GetByteCount(LINE_BREAK);
            }

            length = this.Parameters.Aggregate(length,
                (current, param) => current + this.Encoding.GetByteCount(this.GetMultipartFormData(param)));

            length += this.Encoding.GetByteCount(GetMultipartFooter());

            return length;
        }
#endif

        private void RequestStreamCallback(IAsyncResult result, Action<HttpResponse> callback)
        {
            HttpWebRequest webRequest = (HttpWebRequest)result.AsyncState;

            if (this.timeoutState.TimedOut)
            {
                HttpResponse response = new HttpResponse { ResponseStatus = ResponseStatus.TimedOut };

                ExecuteCallback(response, callback);

                return;
            }

            // write body to request stream
            try
            {
                using (Stream requestStream = webRequest.EndGetRequestStream(result))
                {
                    if (HasFiles || AlwaysMultipartFormData)
                    {
                        WriteMultipartFormData(requestStream);
                    }
                    else if (RequestBodyBytes != null)
                    {
                        requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
                    }
                    else if (RequestBody != null)
                    {
                        WriteStringTo(requestStream, RequestBody);
                    }
                }
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);

                return;
            }

            IAsyncResult asyncResult = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);

            SetTimeout(asyncResult, this.timeoutState);
        }

        private void SetTimeout(IAsyncResult asyncResult, TimeOutState timeOutState)
        {
#if FRAMEWORK
            if (Timeout != 0)
            {
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
                    TimeoutCallback, timeOutState, Timeout, true);
            }
#endif
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (!timedOut)
                return;

            TimeOutState timeoutState = state as TimeOutState;

            if (timeoutState == null)
            {
                return;
            }

            lock (timeoutState)
            {
                timeoutState.TimedOut = true;
            }

            if (timeoutState.Request != null)
            {
                timeoutState.Request.Abort();
            }
        }

        private static void GetRawResponseAsync(IAsyncResult result, Action<HttpWebResponse> callback)
        {
            HttpWebResponse raw;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)result.AsyncState;

                raw = webRequest.EndGetResponse(result) as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw;
                }

                // Check to see if this is an HTTP error or a transport error.
                // In cases where an HTTP error occurs ( status code >= 400 )
                // return the underlying HTTP response, otherwise assume a
                // transport exception (ex: connection timeout) and
                // rethrow the exception

                if (ex.Response is HttpWebResponse)
                {
                    raw = ex.Response as HttpWebResponse;
                }
                else
                {
                    throw;
                }
            }

            callback(raw);

            if (raw != null)
            {
                raw.Close();
            }
        }

        private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
        {
            HttpResponse response = new HttpResponse { ResponseStatus = ResponseStatus.None };

            try
            {
                if (this.timeoutState.TimedOut)
                {
                    response.ResponseStatus = ResponseStatus.TimedOut;
                    ExecuteCallback(response, callback);

                    return;
                }

                GetRawResponseAsync(result, webResponse =>
                                            {
                                                ExtractResponseData(response, webResponse);
                                                ExecuteCallback(response, callback);
                                            });
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }
        }

        private static void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
        {
            PopulateErrorForIncompleteResponse(response);
            callback(response);
        }

        private static void PopulateErrorForIncompleteResponse(HttpResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed && response.ErrorException == null)
            {
                response.ErrorException = response.ResponseStatus.ToWebException();
                response.ErrorMessage = response.ErrorException.Message;
            }
        }

#if SILVERLIGHT || WINDOW_PHONE
        partial void AddAsyncHeaderActions()
        {
#if SILVERLIGHT
            restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
#endif
#if WINDOWS_PHONE
            // WP7 doesn't as of Beta doesn't support a way to set Content-Length either directly
            // or indirectly
            restrictedHeaderActions.Add("Content-Length", (r, v) => { });
#endif
        }
#endif

        // TODO: Try to merge the shared parts between ConfigureWebRequest and ConfigureAsyncWebRequest (quite a bit of code
        // TODO: duplication at the moment).
        private HttpWebRequest ConfigureAsyncWebRequest(string method, Uri url)
        {
#if SILVERLIGHT
            WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
#endif
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.UseDefaultCredentials = UseDefaultCredentials;

#if !WINDOWS_PHONE && !SILVERLIGHT
            webRequest.PreAuthenticate = PreAuthenticate;
#endif
            AppendHeaders(webRequest);
            AppendCookies(webRequest);

            webRequest.Method = method;

            // make sure Content-Length header is always sent since default is -1
#if !WINDOWS_PHONE
            // WP7 doesn't as of Beta doesn't support a way to set this value either directly
            // or indirectly
            if (!HasFiles && !AlwaysMultipartFormData)
            {
                webRequest.ContentLength = 0;
            }
#endif

            if (Credentials != null)
            {
                webRequest.Credentials = Credentials;
            }

#if !SILVERLIGHT
            if (UserAgent.HasValue())
            {
                webRequest.UserAgent = UserAgent;
            }
#endif

#if FRAMEWORK
            if (ClientCertificates != null)
            {
                webRequest.ClientCertificates.AddRange(ClientCertificates);
            }

            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

            webRequest.ServicePoint.Expect100Continue = false;

            if (Timeout != 0)
            {
                webRequest.Timeout = Timeout;
            }

            if (ReadWriteTimeout != 0)
            {
                webRequest.ReadWriteTimeout = ReadWriteTimeout;
            }

            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }

            if (CachePolicy != null)
            {
                webRequest.CachePolicy = CachePolicy;
            }

            if (FollowRedirects && MaxRedirects.HasValue)
            {
                webRequest.MaximumAutomaticRedirections = MaxRedirects.Value;
            }
#endif

#if !SILVERLIGHT
            webRequest.AllowAutoRedirect = FollowRedirects;
#endif
            return webRequest;
        }

        private class TimeOutState
        {
            public bool TimedOut { get; set; }

            public HttpWebRequest Request { get; set; }
        }
    }
}
