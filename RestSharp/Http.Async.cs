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

		private void GetStyleMethodInternalAsync(string method, Action<HttpResponse> action)
		{
			var webRequest = ConfigureAsyncWebRequest(method, Url);
			webRequest.BeginGetResponse(result => ResponseCallback(result, action), webRequest);
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

				callback(response);
			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				response.ResponseStatus = ResponseStatus.Error;
			}
		}

		private void PutPostInternalAsync(string method, Action<HttpResponse> action)
		{
			var webRequest = ConfigureAsyncWebRequest(method, Url);
			//PreparePostData(webRequest);
			//WriteRequestBody(webRequest);
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
			webRequest.Method = method;

			if (Credentials != null)
			{
				webRequest.Credentials = Credentials;
			}

			return webRequest;
		}
	}
}
