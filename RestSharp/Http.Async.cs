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
				var webRequest = ConfigureAsyncWebRequest(method, Url);
				webRequest.BeginGetResponse(result => ResponseCallback(result, callback), webRequest);
			}
			catch (Exception ex)
			{
				var response = new HttpResponse();
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
				callback(response);
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
				callback(response);
			}
		}

		private void WriteRequestBodyAsync(HttpWebRequest webRequest, Action<HttpResponse> callback)
		{
			if (HasBody)
			{
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

		private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
		{
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			try
			{
				var webRequest = result.AsyncState as HttpWebRequest;
				var webResponse = webRequest.EndGetResponse(result) as HttpWebResponse;

				ExtractResponseData(response, webResponse);

				ExecuteCallback(response, callback);
			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
			}
		}

		private void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
		{
#if WINDOWS_PHONE
			var dispatcher = Deployment.Current.Dispatcher;
			dispatcher.BeginInvoke(() => {
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

			if (Credentials != null)
			{
				webRequest.Credentials = Credentials;
			}

			return webRequest;
		}
	}
}
