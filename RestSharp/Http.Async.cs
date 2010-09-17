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
using System.Web;
#endif

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper (async methods)
	/// </summary>
	public partial class Http
	{
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
				var url = AssembleUrl();
				var webRequest = ConfigureAsyncWebRequest(method, url);
				webRequest.BeginGetResponse(result => ResponseCallback(result, callback), webRequest);
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
			if (HasBody)
			{
#if !WINDOWS_PHONE
				webRequest.ContentLength = RequestBody.Length;
#endif

				webRequest.BeginGetRequestStream(result => RequestStreamCallback(result, callback), webRequest);
			}
			else
			{
				webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
			}
		}

		private void RequestStreamCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var webRequest = result.AsyncState as HttpWebRequest;

			// write body to request stream
			using (var requestStream = webRequest.EndGetRequestStream(result))
			{
				var encoding = Encoding.UTF8;
				requestStream.Write(encoding.GetBytes(RequestBody), 0, RequestBody.Length);
			}

			webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
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
		}

		private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			try
			{
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
			//var dispatcher = Deployment.Current.Dispatcher;
			//dispatcher.BeginInvoke(() => {
#endif
			callback(response);
#if WINDOWS_PHONE
			//});
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

			AppendHeaders(webRequest);
			AppendCookies(webRequest);

			webRequest.Method = method;

			// make sure Content-Length header is always sent since default is -1
#if !WINDOWS_PHONE
			// WP7 doesn't as of Beta doesn't support a way to set this value either directly
			// or indirectly
			webRequest.ContentLength = 0;
#endif
			if (Credentials != null)
			{
				webRequest.Credentials = Credentials;
			}

			return webRequest;
		}
	}
}
