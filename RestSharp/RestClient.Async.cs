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
using System.Linq;
using System.Threading;
using System.Text;
using System.Net;

namespace RestSharp
{
	public partial class RestClient
	{
		/// <summary>
		/// Executes the request and callback asynchronously, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		/// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
		public virtual RestRequestAsyncHandle ExecuteAsync(IRestRequest request, Action<IRestResponse, RestRequestAsyncHandle> callback)
		{
			var http = HttpFactory.Create();
			AuthenticateIfNeeded(this, request);

			// add Accept header based on registered deserializers
			var accepts = string.Join(", ", AcceptTypes.ToArray());
			AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader);

			ConfigureHttp(request, http);

			HttpWebRequest webRequest = null;
			var asyncHandle = new RestRequestAsyncHandle();

			Action<HttpResponse> response_cb = r => ProcessResponse(r, asyncHandle, callback);

			if (UseSynchronizationContext && SynchronizationContext.Current != null) {
				var ctx = SynchronizationContext.Current;
				var cb = response_cb;

				response_cb = resp => ctx.Post(s => cb(resp), null);
			}
			
			switch(request.Method)
			{
				case Method.GET:
					webRequest = http.GetAsync(response_cb);
					break;
				case Method.POST:
					webRequest = http.PostAsync(response_cb);
					break;
				case Method.PUT:
					webRequest = http.PutAsync(response_cb);
					break;
				case Method.DELETE:
					webRequest = http.DeleteAsync(response_cb);
					break;
				case Method.HEAD:
					webRequest = http.HeadAsync(response_cb);
					break;
				case Method.OPTIONS:
					webRequest = http.OptionsAsync(response_cb);
					break;
				case Method.PATCH:
					webRequest = http.PatchAsync(response_cb);
					break;
			}
			
			asyncHandle.WebRequest = webRequest;
			return asyncHandle;
		}

		private void ProcessResponse(HttpResponse httpResponse, RestRequestAsyncHandle asyncHandle, Action<IRestResponse, RestRequestAsyncHandle> callback)
		{
			var restResponse = ConvertToRestResponse(httpResponse);
			callback(restResponse, asyncHandle);
		}

		/// <summary>
		/// Executes the request and callback asynchronously, authenticating if needed
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to be executed</param>
		/// <param name="callback">Callback function to be executed upon completion</param>
		public virtual RestRequestAsyncHandle ExecuteAsync<T>(IRestRequest request, Action<IRestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			return ExecuteAsync(request, (response, asyncHandle) =>
			{
				var restResponse = (IRestResponse<T>)response;
				if (response.ResponseStatus != ResponseStatus.Aborted)
				{
					restResponse = Deserialize<T>(request, response);
				}

				callback(restResponse, asyncHandle);
			});
		}
	}
}