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
		public void ExecuteAsync(RestRequest request, Action<RestResponse> callback)
		{
			AuthenticateIfNeeded(request);

			var http = new Http();
			ConfigureHttp(request, http);

			switch (request.Method)
			{
				case Method.GET:
					http.GetAsync(r => ProcessResponse(r, callback));
					break;
				case Method.POST:
					http.PostAsync(r => ProcessResponse(r, callback));
					break;
				case Method.PUT:
					http.PutAsync(r => ProcessResponse(r, callback));
					break;
				case Method.DELETE:
					http.DeleteAsync(r => ProcessResponse(r, callback));
					break;
				case Method.HEAD:
					http.HeadAsync(r => ProcessResponse(r, callback));
					break;
				case Method.OPTIONS:
					http.OptionsAsync(r => ProcessResponse(r, callback));
					break;
			}
		}

		void ProcessResponse(HttpResponse httpResponse, Action<RestResponse> callback)
		{
			var restResponse = ConvertToRestResponse(httpResponse);
			callback(restResponse);
		}

		public void ExecuteAsync<T>(RestRequest request, Action<RestResponse<T>> callback) where T : new()
		{
			ExecuteAsync(request, response =>
				{
					var restResponse = Deserialize<T>(request, response);
					callback(restResponse);
				}
			);
		}
	}
}