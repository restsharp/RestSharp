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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    ///     HttpWebRequest wrapper (async methods)
    /// </summary>
    public partial class Http
    {
        TimeOutState _timeoutState;

        /// <summary>
        ///     Execute an async POST-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
            => PutPostInternalAsync(httpMethod.ToUpperInvariant(), action);

        /// <summary>
        ///     Execute an async GET-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
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

        async Task<HttpResponse> AsyncTest(WebRequest webRequest, CancellationToken cancellationToken)
        {
            var ct = Timeout > 0 ? GetTimeoutToken() : cancellationToken;

            using var requestStream = await webRequest.GetRequestStreamAsync(ct);

            if (HasFiles || AlwaysMultipartFormData)
                await WriteMultipartFormDataAsync(requestStream, ct);
            else if (RequestBodyBytes != null)
                await requestStream.WriteAsync(RequestBodyBytes, 0, RequestBodyBytes.Length, ct);
            else if (RequestBody != null)
                await requestStream.WriteStringAsync(RequestBody, Encoding, ct);

            try
            {
                using var webResponse = await webRequest.GetResponseAsync(ct);

                return ExtractResponseData((HttpWebResponse) webResponse);
            }
            catch (Exception e)
            {
                return CreateErrorResponse(e);
            }

            CancellationToken GetTimeoutToken()
            {
                var timeoutTokenSource = new CancellationTokenSource(Timeout);
                var timeoutToken       = timeoutTokenSource.Token;
                return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken).Token;
            }
        }

        async Task WriteMultipartFormDataAsync(Stream requestStream, CancellationToken cancellationToken)
        {
            foreach (var param in Parameters)
                await requestStream.WriteStringAsync(GetMultipartFormData(param), Encoding, cancellationToken);

            foreach (var file in Files)
            {
                // Add just the first part of this param, since we will write the file data directly to the Stream
                await requestStream.WriteStringAsync(GetMultipartFileHeader(file), Encoding, cancellationToken);

                // Write the file data directly to the Stream, rather than serializing it to a string.
                file.Writer(requestStream);
                await requestStream.WriteStringAsync(LineBreak, Encoding, cancellationToken);
            }

            await requestStream.WriteStringAsync(GetMultipartFooter(), Encoding, cancellationToken);
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