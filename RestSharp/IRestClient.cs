﻿#region License
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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace RestSharp
{
	/// <summary>
	/// 
	/// </summary>
	public interface IRestClient
	{
		/// <summary>
		/// 
		/// </summary>
		CookieContainer CookieContainer { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string UserAgent { get; set; }
		/// <summary>
		/// 
		/// </summary>
		int Timeout { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool UseSynchronizationContext { get; set; }
		/// <summary>
		/// 
		/// </summary>
		IAuthenticator Authenticator { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string BaseUrl { get; set; }
		/// <summary>
		/// 
		/// </summary>
		IList<Parameter> DefaultParameters { get; }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
        RestRequestAsyncHandle ExecuteAsync(IRestRequest request, Action<IRestResponse, RestRequestAsyncHandle> callback, object userState = null);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
        RestRequestAsyncHandle ExecuteAsync<T>(IRestRequest request, Action<IRestResponse<T>, RestRequestAsyncHandle> callback, object userState = null);

#if FRAMEWORK
		/// <summary>
		/// X509CertificateCollection to be sent with request
		/// </summary>
		X509CertificateCollection ClientCertificates { get; set; }
		IRestResponse Execute(IRestRequest request);
		IRestResponse<T> Execute<T>(IRestRequest request) where T : new();
		
		IWebProxy Proxy { get; set; }
#endif

		Uri BuildUri(IRestRequest request);
	}
}
