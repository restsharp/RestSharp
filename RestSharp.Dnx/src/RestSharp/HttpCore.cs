using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Extensions;

namespace RestSharp
{
    public class HttpCore : IHttp, IHttpFactory
    {
        private TimeOutState timeoutState;
        private HttpClient _client;

        public IHttp Create()
        {
            return new HttpCore();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HttpCore()
        {
            //Headers = new List<HttpHeader>();
            //Files = new List<HttpFile>();
            //Parameters = new List<HttpParameter>();
            //Cookies = new List<HttpCookie>();
            //this.restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase);

            //this.AddSharedHeaderActions();
            //this.AddSyncHeaderActions();
            _client = new HttpClient();
        }

        public bool AlwaysMultipartFormData { get; set; }

        public CookieContainer CookieContainer { get; set; }

        public IList<HttpCookie> Cookies { get; set; }

        public ICredentials Credentials { get; set; }

        public Encoding Encoding { get; set; }

        public IList<HttpFile> Files { get; set; }

        public bool FollowRedirects { get; set; }

        public IList<HttpHeader> Headers { get; set; }

        public IList<HttpParameter> Parameters { get; set; }

        public bool PreAuthenticate { get; set; }

        public int ReadWriteTimeout { get; set; }

        public string RequestBody { get; set; }

        public byte[] RequestBodyBytes { get; set; }

        public string RequestContentType { get; set; }

        public Action<Stream> ResponseWriter { get; set; }

        public int Timeout { get; set; }

        public Uri Url { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public string UserAgent { get; set; }

        public HttpWebRequest AsGetAsync(Action<HttpResponse> callback, string httpMethod)
        {
            HttpWebRequest webRequest = null;

            //try
            //{
            //    Uri url = Url;

            //    webRequest = this.ConfigureAsyncWebRequest(method, url);

            //    if (this.HasBody && (method == "DELETE" || method == "OPTIONS"))
            //    {
            //        webRequest.ContentType = this.RequestContentType;
            //        this.WriteRequestBodyAsync(webRequest, callback);
            //    }
            //    else
            //    {
            //        this.timeoutState = new TimeOutState { Request = webRequest };

            //        IAsyncResult asyncResult = webRequest.BeginGetResponse(
            //            result => this.ResponseCallback(result, callback), webRequest);

            //        this.SetTimeout(asyncResult, this.timeoutState);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ExecuteCallback(this.CreateErrorResponse(ex), callback);
            //}

            return webRequest;
        }

        public HttpWebRequest AsPostAsync(Action<HttpResponse> callback, string httpMethod)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest DeleteAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest GetAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest HeadAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest MergeAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest OptionsAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest PatchAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest PostAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest PutAsync(Action<HttpResponse> callback)
        {
            throw new NotImplementedException();
        }

        #region
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
                    // todo this.ExtractResponseData(response, webResponse);
                    ExecuteCallback(response, callback);
                });
            }
            catch (Exception ex)
            {
                ExecuteCallback(this.CreateErrorResponse(ex), callback);
            }
        }

        private static void GetRawResponseAsync(IAsyncResult result, Action<HttpWebResponse> callback)
        {
            HttpWebResponse raw = null;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)result.AsyncState;

                raw = null; // todo webRequest.EndGetResponse(result) as HttpWebResponse;
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

                //if (ex.Response is HttpWebResponse)
                //{
                //    raw = ex.Response as HttpWebResponse;
                //}
                //else
                //{
                //    throw;
                //}
            }

            callback(raw);

            if (raw != null)
            {
#if !WINDOWS_UWP
                raw.Close();
#else
                raw.Dispose();
#endif
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

        private HttpResponse CreateErrorResponse(Exception ex)
        {
            HttpResponse response = new HttpResponse();
            WebException webException = ex as WebException;

            if (webException != null && webException.Status == WebExceptionStatus.RequestCanceled)
            {
                response.ResponseStatus = this.timeoutState.TimedOut
                    ? ResponseStatus.TimedOut
                    : ResponseStatus.Aborted;

                return response;
            }

            response.ErrorMessage = ex.Message;
            response.ErrorException = ex;
            response.ResponseStatus = ResponseStatus.Error;

            return response;
        }
        #endregion

        private class TimeOutState
        {
            public bool TimedOut { get; set; }

            public HttpWebRequest Request { get; set; }
        }
    }
}