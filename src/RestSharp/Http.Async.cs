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
using System.Linq;
using System.Net;
using System.Threading;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    ///     HttpWebRequest wrapper (async methods)
    /// </summary>
    public partial class Http
    {
        TimeOutState _timeoutState;

        /// <inheritdoc />
        public HttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
            => PutPostInternalAsync(httpMethod.ToUpperInvariant(), action);

        /// <inheritdoc />
        public HttpWebRequest AsGetAsync(Action<HttpResponse> action, string httpMethod)
            => GetStyleMethodInternalAsync(httpMethod.ToUpperInvariant(), action);

        HttpWebRequest GetStyleMethodInternalAsync(string method, Action<HttpResponse> callback)
        {
            HttpWebRequest webRequest = null;

            try
            {
                webRequest = ConfigureAsyncWebRequest(method, Url);

                if (HasBody && (method == "DELETE" || method == "OPTIONS"))
                {
                    webRequest.ContentType = RequestContentType;
                    WriteRequestBodyAsync(webRequest, callback);
                }
                else
                {
                    // webRequest.GetResponseAsync();

                    _timeoutState = new TimeOutState {Request = webRequest};

                    var asyncResult = webRequest.BeginGetResponse(
                        result => ResponseCallback(result, callback), webRequest
                    );

                    SetTimeout(asyncResult);
                }
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }

            return webRequest;
        }

        HttpWebRequest PutPostInternalAsync(string method, Action<HttpResponse> callback)
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

        void WriteRequestBodyAsync(HttpWebRequest webRequest, Action<HttpResponse> callback)
        {
            IAsyncResult asyncResult;
            _timeoutState = new TimeOutState {Request = webRequest};

            if (HasBody || HasFiles || AlwaysMultipartFormData)
            {
                webRequest.ContentLength = CalculateContentLength();

                asyncResult = webRequest.BeginGetRequestStream(
                    RequestStreamCallback, webRequest
                );
            }
            else
            {
                asyncResult = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
            }

            SetTimeout(asyncResult);

            void RequestStreamCallback(IAsyncResult result)
            {
                if (_timeoutState.TimedOut)
                {
                    var response = new HttpResponse {ResponseStatus = ResponseStatus.TimedOut};

                    ExecuteCallback(response, callback);

                    return;
                }

                // write body to request stream
                try
                {
                    using (var requestStream = webRequest.EndGetRequestStream(result))
                    {
                        if (HasFiles || AlwaysMultipartFormData)
                            WriteMultipartFormData(requestStream);
                        else if (RequestBodyBytes != null)
                            requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
                        else if (RequestBody != null)
                            requestStream.WriteString(RequestBody, Encoding);
                    }

                    var response = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);

                    SetTimeout(response);
                }
                catch (Exception ex)
                {
                    ExecuteCallback(CreateErrorResponse(ex), callback);
                }
            }
        }

        long CalculateContentLength()
        {
            if (RequestBodyBytes != null)
                return RequestBodyBytes.Length;

            if (!HasFiles && !AlwaysMultipartFormData)
                return Encoding.GetByteCount(RequestBody);

            // calculate length for multipart form
            long length = 0;

            foreach (var file in Files)
            {
                length += Encoding.GetByteCount(GetMultipartFileHeader(file));
                length += file.ContentLength;
                length += Encoding.GetByteCount(LineBreak);
            }

            length = Parameters.Aggregate(
                length,
                (current, param) => current + Encoding.GetByteCount(GetMultipartFormData(param))
            );

            length += Encoding.GetByteCount(GetMultipartFooter());

            return length;
        }

        void SetTimeout(IAsyncResult asyncResult)
        {
            if (Timeout != 0)
                ThreadPool.RegisterWaitForSingleObject(
                    asyncResult.AsyncWaitHandle,
                    TimeoutCallback, _timeoutState, Timeout, true
                );

            static void TimeoutCallback(object state, bool timedOut)
            {
                if (!timedOut)
                    return;

                if (!(state is TimeOutState tos))
                    return;

                lock (tos) tos.TimedOut = true;

                tos.Request?.Abort();
            }
        }

        static void GetRawResponseAsync(IAsyncResult result, Action<HttpWebResponse> callback)
        {
            HttpWebResponse raw;

            try
            {
                var webRequest = (HttpWebRequest) result.AsyncState;

                raw = webRequest.EndGetResponse(result) as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                    throw;

                // Check to see if this is an HTTP error or a transport error.
                // In cases where an HTTP error occurs ( status code >= 400 )
                // return the underlying HTTP response, otherwise assume a
                // transport exception (ex: connection timeout) and
                // rethrow the exception

                if (ex.Response is HttpWebResponse response)
                    raw = response;
                else
                    throw;
            }

            callback(raw);

            raw?.Close();
        }

        void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
        {
            try
            {
                if (_timeoutState.TimedOut)
                {
                    var response = new HttpResponse {ResponseStatus = ResponseStatus.TimedOut};
                    ExecuteCallback(response, callback);

                    return;
                }

                GetRawResponseAsync(
                    result, webResponse =>
                    {
                        var response = ExtractResponseData(webResponse);
                        webResponse.Dispose();
                        ExecuteCallback(response, callback);
                    }
                );
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }
        }

        static void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
        {
            if (response.ResponseStatus != ResponseStatus.Completed && response.ErrorException == null)
            {
                response.ErrorException = response.ResponseStatus.ToWebException();
                response.ErrorMessage   = response.ErrorException.Message;
            }

            callback(response);
        }

        HttpResponse CreateErrorResponse(Exception ex)
        {
            var response = new HttpResponse {ErrorMessage = ex.Message, ErrorException = ex};

            if (ex is WebException webException)
            {
                response.ResponseStatus = webException.Status switch
                {
                    WebExceptionStatus.RequestCanceled => _timeoutState.TimedOut ? ResponseStatus.TimedOut : ResponseStatus.Aborted,
                    WebExceptionStatus.Timeout         => ResponseStatus.TimedOut,
                    _                                  => ResponseStatus.Error
                };
            }
            else
                response.ResponseStatus = ResponseStatus.Error;

            return response;
        }
        
        class TimeOutState
        {
            public bool TimedOut { get; set; }

            public HttpWebRequest Request { get; set; }
        }
    }
}