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
		public virtual RestRequestAsyncHandle ExecuteAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
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

		private void ProcessResponse(HttpResponse httpResponse, RestRequestAsyncHandle asyncHandle, Action<RestResponse, RestRequestAsyncHandle> callback)
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
		public virtual RestRequestAsyncHandle ExecuteAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			return ExecuteAsync(request, (response, asyncHandle) =>
			{
				var restResponse = (RestResponse<T>)response;
				if (response.ResponseStatus != ResponseStatus.Aborted)
				{
					restResponse = Deserialize<T>(request, response);
				}

				callback(restResponse, asyncHandle);
			});
		}

		public virtual RestRequestAsyncHandle GetAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.GET;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle PostAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.POST;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle PutAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.PUT;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle HeadAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.HEAD;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle OptionsAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.OPTIONS;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle PatchAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.PATCH;
			return ExecuteAsync<T>(request, callback);
		}

		public virtual RestRequestAsyncHandle DeleteAsync<T>(IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.DELETE;
			return ExecuteAsync<T>(request, callback);
		}


		public virtual RestRequestAsyncHandle GetAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.GET;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle PostAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.POST;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle PutAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.PUT;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle HeadAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.HEAD;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle OptionsAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.OPTIONS;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle PatchAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.PATCH;
			return ExecuteAsync(request, callback);
		}

		public virtual RestRequestAsyncHandle DeleteAsync(IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.DELETE;
			return ExecuteAsync(request, callback);
		}

	}
}