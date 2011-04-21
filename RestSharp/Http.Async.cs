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

#if FRAMEWORK
	#if !MONOTOUCH
using System.Web;
	#endif
#endif

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper (async methods)
	/// </summary>
	public partial class Http
	{
		private TimeOutState timeoutState = null;

		public void DeleteAsync(Action<HttpResponse> action)
		{
			GetStyleMethodInternalAsync("DELETE", action);
		}

		public void GetAsync(Action<HttpResponse> action)
		{
			GetStyleMethodInternalAsync("GET", action);
		}

		public void HeadAsync(Action<HttpResponse> action)
		{
			GetStyleMethodInternalAsync("HEAD", action);
		}

		public void OptionsAsync(Action<HttpResponse> action)
		{
			GetStyleMethodInternalAsync("OPTIONS", action);
		}

		public void PostAsync(Action<HttpResponse> action)
		{
			PutPostInternalAsync("POST", action);
		}

		public void PutAsync(Action<HttpResponse> action)
		{
			PutPostInternalAsync("PUT", action);
		}

		private void GetStyleMethodInternalAsync(string method, Action<HttpResponse> callback)
		{
			try
			{
				var url = Url;
				var webRequest = ConfigureAsyncWebRequest(method, url);
				timeoutState = new TimeOutState { Request = webRequest };
				var asyncResult = webRequest.BeginGetResponse(result => ResponseCallback(result, callback), webRequest);
				SetTimeout(asyncResult, webRequest, timeoutState);
			}
			catch (Exception ex)
			{
				var response = new HttpResponse();
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
		}

		private void PutPostInternalAsync(string method, Action<HttpResponse> callback)
		{
			try
			{
				var webRequest = ConfigureAsyncWebRequest(method, Url);
				PreparePostBody(webRequest);
				WriteRequestBodyAsync(webRequest, callback);
			}
			catch (Exception ex)
			{
				var response = new HttpResponse();
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
		}

		private void WriteRequestBodyAsync(HttpWebRequest webRequest, Action<HttpResponse> callback)
		{
			IAsyncResult asyncResult;
			timeoutState = new TimeOutState { Request = webRequest };

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

			SetTimeout(asyncResult, webRequest, timeoutState);
		}

		private long CalculateContentLength()
		{
			if (!HasFiles)
			{
				return RequestBody.Length;
			}

			// calculate length for multipart form
			long length = 0;
			foreach (var file in Files)
			{
				length += GetMultipartFileHeader(file).Length;
				length += file.ContentLength;
				length += Environment.NewLine.Length;
			}

			foreach (var param in Parameters)
			{
				length += GetMultipartFormData(param).Length;
			}

			length += GetMultipartFooter().Length;
			return length;
		}

		private void WriteMultipartFormDataAsync(Stream requestStream)
		{
			var encoding = Encoding.UTF8;
			foreach (var file in Files)
			{
				// Add just the first part of this param, since we will write the file data directly to the Stream
				var header = GetMultipartFileHeader(file);
				requestStream.Write(encoding.GetBytes(header), 0, header.Length);

				// Write the file data directly to the Stream, rather than serializing it to a string.
				file.Writer(requestStream);
				var lineEnding = Environment.NewLine;
				requestStream.Write(encoding.GetBytes(lineEnding), 0, lineEnding.Length);
			}

			foreach (var param in Parameters)
			{
				var postData = GetMultipartFormData(param);
				requestStream.Write(encoding.GetBytes(postData), 0, postData.Length);
			}

			var footer = GetMultipartFooter();
			requestStream.Write(encoding.GetBytes(footer), 0, footer.Length);
		}

		private void RequestStreamCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var webRequest = result.AsyncState as HttpWebRequest;

			// write body to request stream
			using (var requestStream = webRequest.EndGetRequestStream(result))
			{
				if (HasFiles)
				{
					WriteMultipartFormDataAsync(requestStream);
				}



				else
				{
					var encoding = Encoding.UTF8;
					requestStream.Write(encoding.GetBytes(RequestBody), 0, RequestBody.Length);
				}
			}

			webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
		}

		private void SetTimeout(IAsyncResult asyncResult, HttpWebRequest request, TimeOutState timeOutState)
		{
#if FRAMEWORK
			if (Timeout != 0)
			{
				ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), timeOutState, Timeout, true);
			} 
#endif		
		}

		private void TimeoutCallback(object state, bool timedOut)
		{
			if (timedOut)
			{
				TimeOutState timeoutState = state as TimeOutState;

				if (timeoutState == null)
				{
					return;
				}

				lock (timeoutState)
				{
					timeoutState.TimedOut = timedOut;
				}

				if (timeoutState.Request != null)
				{
					timeoutState.Request.Abort();
				}
			}
		}

		private void GetRawResponseAsync(IAsyncResult result, Action<HttpWebResponse> callback)
		{
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			HttpWebResponse raw = null;

			try
			{
				var webRequest = (HttpWebRequest)result.AsyncState;
				raw = webRequest.EndGetResponse(result) as HttpWebResponse;
			}
			catch (WebException ex)
			{
				if (ex.Response is HttpWebResponse)
				{
					raw = ex.Response as HttpWebResponse;
				}
			}

			callback(raw);
			raw.Close();
		}

		private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			try
			{
				if (timeoutState.TimedOut)
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
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				ExecuteCallback(response, callback);
			}
		}

		private void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
		{
#if WINDOWS_PHONE
			var dispatcher = Deployment.Current.Dispatcher;
			dispatcher.BeginInvoke(() =>
			{
#endif
			callback(response);
#if WINDOWS_PHONE
			});
#endif
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
			if (!HasFiles)
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
