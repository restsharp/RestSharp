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
using System.Net;
using System.Threading;
using RestSharp.Extensions;

#if SILVERLIGHT
using System.Windows.Browser;
using System.Net.Browser;
#endif

#if WINDOWS_PHONE
using System.Windows.Threading;
using System.Windows;
#endif

#if (FRAMEWORK && !MONOTOUCH && !MONODROID)
using System.Web;
#endif

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper (async methods)
	/// </summary>
	public partial class Http
	{
		private TimeOutState _timeoutState;

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

		/// <summary>
		/// Execute an async POST-style request with the specified HTTP Method.  
		/// </summary>
		/// <param name="httpMethod">The HTTP method to execute.</param>
		/// <returns></returns>
		public HttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
		{
			return PutPostInternalAsync(httpMethod.ToUpperInvariant(), action);
		}

		/// <summary>
		/// Execute an async GET-style request with the specified HTTP Method.  
		/// </summary>
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
				var url = Url;
				webRequest = ConfigureAsyncWebRequest(method, url);
				_timeoutState = new TimeOutState { Request = webRequest };
				var asyncResult = webRequest.BeginGetResponse(result => ResponseCallback(result, callback), webRequest);
				SetTimeout(asyncResult, _timeoutState);
			}
			catch(Exception ex)
			{
				var response = new HttpResponse();
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
			return webRequest;
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
			catch(Exception ex)
			{
				var response = new HttpResponse();
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
			
			return webRequest;
		}

		private void WriteRequestBodyAsync(HttpWebRequest webRequest, Action<HttpResponse> callback)
		{
			IAsyncResult asyncResult;
			_timeoutState = new TimeOutState { Request = webRequest };

			if (HasBody || HasFiles)
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

			SetTimeout(asyncResult, _timeoutState);
		}

		private long CalculateContentLength()
		{
			if (RequestBodyBytes != null)
				return RequestBodyBytes.Length;

			if (!HasFiles)
			{
				return _defaultEncoding.GetByteCount(RequestBody);
			}

			// calculate length for multipart form
			long length = 0;
			foreach (var file in Files)
			{
				length += _defaultEncoding.GetByteCount(GetMultipartFileHeader(file));
				length += file.ContentLength;
				length += _defaultEncoding.GetByteCount(_lineBreak);
			}

			foreach (var param in Parameters)
			{
				length += _defaultEncoding.GetByteCount(GetMultipartFormData(param));
			}

			length += _defaultEncoding.GetByteCount(GetMultipartFooter());
			return length;
		}

		private void RequestStreamCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var webRequest = (HttpWebRequest)result.AsyncState;

			if (_timeoutState.TimedOut)
			{
				var response = new HttpResponse {ResponseStatus = ResponseStatus.TimedOut};
				ExecuteCallback(response, callback);
				return;
			}

			// write body to request stream
			try
			{
				using(var requestStream = webRequest.EndGetRequestStream(result))
				{
					if(HasFiles)
					{
						WriteMultipartFormData(requestStream);
					}
					else if (RequestBodyBytes != null)
					{
						requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
					}
					else
					{
						WriteStringTo(requestStream, RequestBody);
					}
				}
			}
			catch (Exception ex)
			{
				HttpResponse response;
				if (ex is WebException && ((WebException)ex).Status == WebExceptionStatus.RequestCanceled)
				{
					response = new HttpResponse {ResponseStatus = ResponseStatus.TimedOut};
					ExecuteCallback (response, callback);
					return;
				}
				
				response = new HttpResponse
				{
					ErrorMessage = ex.Message,
					ErrorException = ex,
					ResponseStatus = ResponseStatus.Error
				};
				ExecuteCallback(response, callback);
				return;
			}

			webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
		}

		private void SetTimeout(IAsyncResult asyncResult, TimeOutState timeOutState)
		{
#if FRAMEWORK
			if (Timeout != 0)
			{
				ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), timeOutState, Timeout, true);
			} 
#endif		
		}

		private static void TimeoutCallback(object state, bool timedOut)
		{
			if (!timedOut)
				return;

			var timeoutState = state as TimeOutState;

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
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			HttpWebResponse raw = null;

			try
			{
				var webRequest = (HttpWebRequest)result.AsyncState;
				raw = webRequest.EndGetResponse(result) as HttpWebResponse;
			}
			catch(WebException ex)
			{
				if(ex.Status == WebExceptionStatus.RequestCanceled)
				{
					throw ex;
				}
				if (ex.Response is HttpWebResponse)
				{
					raw = ex.Response as HttpWebResponse;
				}
				else
				{
					throw ex;
				}
			}

			callback(raw);
			raw.Close();
		}

		private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var response = new HttpResponse {ResponseStatus = ResponseStatus.None};

			try
			{
				if(_timeoutState.TimedOut)
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
			catch(Exception ex)
			{
				if(ex is WebException && ((WebException)ex).Status == WebExceptionStatus.RequestCanceled)
				{
					response.ResponseStatus = ResponseStatus.Aborted;
					ExecuteCallback(response, callback);
					return;
				}

				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
		}

		private static void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
		{
			callback(response);
		}

		partial void AddAsyncHeaderActions()
		{
#if SILVERLIGHT
			_restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
#endif
#if WINDOWS_PHONE
			// WP7 doesn't as of Beta doesn't support a way to set Content-Length either directly
			// or indirectly
			_restrictedHeaderActions.Add("Content-Length", (r, v) => { });
#endif
		}

		private HttpWebRequest ConfigureAsyncWebRequest(string method, Uri url)
		{
#if SILVERLIGHT
			WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
			WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
#endif
			var webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.UseDefaultCredentials = false;

			AppendHeaders(webRequest);
			AppendCookies(webRequest);

			webRequest.Method = method;

			// make sure Content-Length header is always sent since default is -1
#if !WINDOWS_PHONE
			// WP7 doesn't as of Beta doesn't support a way to set this value either directly
			// or indirectly
			if(!HasFiles)
			{
				webRequest.ContentLength = 0;
			}
#endif
	
			if(Credentials != null)
			{
				webRequest.Credentials = Credentials;
			}

#if !SILVERLIGHT
			if(UserAgent.HasValue())
			{
				webRequest.UserAgent = UserAgent;
			}
#endif

#if FRAMEWORK
			if(ClientCertificates != null)
			{
				webRequest.ClientCertificates = ClientCertificates;
			}
			
			webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;
			ServicePointManager.Expect100Continue = false;

			if (Timeout != 0)
			{
				webRequest.Timeout = Timeout;
			}

			if (Proxy != null)
			{
				webRequest.Proxy = Proxy;
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
