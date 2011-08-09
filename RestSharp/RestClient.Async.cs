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
		/// <param name="callback">Callback function to be executed upon completion</param>
		public virtual RestRequestAsyncHandle ExecuteAsync(RestRequest request, Action<RestResponse> callback)
		{
			var http = HttpFactory.Create();
			AuthenticateIfNeeded(this, request);

			ConfigureHttp(request, http);

			// add Accept header based on registered deserializers
			var accepts = string.Join(", ", AcceptTypes.ToArray());
			AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader);
			HttpWebRequest webRequest = null;
			
			switch(request.Method)
			{
				case Method.GET:
					webRequest = http.GetAsync(r => ProcessResponse(r, callback));
					break;
				case Method.POST:
					webRequest = http.PostAsync(r => ProcessResponse(r, callback));
					break;
				case Method.PUT:
					webRequest = http.PutAsync(r => ProcessResponse(r, callback));
					break;
				case Method.DELETE:
					webRequest = http.DeleteAsync(r => ProcessResponse(r, callback));
					break;
				case Method.HEAD:
					webRequest = http.HeadAsync(r => ProcessResponse(r, callback));
					break;
				case Method.OPTIONS:
					webRequest = http.OptionsAsync(r => ProcessResponse(r, callback));
					break;
			}
			
			return new RestRequestAsyncHandle(webRequest);
		}

		void ProcessResponse(HttpResponse httpResponse, Action<RestResponse> callback)
		{
			var restResponse = ConvertToRestResponse(httpResponse);
			callback(restResponse);
		}

		/// <summary>
		/// Executes the request and callback asynchronously, authenticating if needed
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to be executed</param>
		/// <param name="callback">Callback function to be executed upon completion</param>
		public virtual RestRequestAsyncHandle ExecuteAsync<T>(RestRequest request, Action<RestResponse<T>> callback) where T : new()
		{
			return ExecuteAsync(request, response =>
			{
				var restResponse = (RestResponse<T>)response;
				if(response.ResponseStatus != ResponseStatus.Aborted)
				{
					restResponse = Deserialize<T>(request, response);
				}

				callback(restResponse);
			});
		}
	}
}