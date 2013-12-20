#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
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
using System.Threading;
using System.Threading.Tasks;

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
		int? Timeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool FollowRedirects { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int? MaxRedirects { get; set; }

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
        ICollection<string> DefaultAcceptTypes { get; }

        /// <summary>
		/// X509CertificateCollection to be sent with request
		/// </summary>
		//X509CertificateCollection ClientCertificates { get; set; }
		
        /// <summary>
        /// 
        /// </summary>
		IWebProxy Proxy { get; set; }

		/// <summary>
		/// Executes the request and callback asynchronously, authenticating if needed
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to be executed</param>
		/// <param name="token">The cancellation token</param>
		Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken token);

		/// <summary>
		/// Executes the request asynchronously, authenticating if needed
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to be executed</param>
		Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request);

		/// <summary>
		/// Executes the request and callback asynchronously, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		/// <param name="token">The cancellation token</param>
		Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken token);

		/// <summary>
		/// Executes the request asynchronously, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		Task<IRestResponse> ExecuteAsync(IRestRequest request);

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse<T>> ExecuteGetAsync<T>(IRestRequest request);

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse<T>> ExecutePostAsync<T>(IRestRequest request);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken token);


	}
}
