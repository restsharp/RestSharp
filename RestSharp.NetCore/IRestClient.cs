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
using System.Net;
using System.Collections.Generic;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

#if NET4 || MONODROID || MONOTOUCH || WP8 || WINDOWS_UWP
using System.Threading;
using System.Threading.Tasks;
#endif

#if FRAMEWORK
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#endif

namespace RestSharp
{
    public interface IRestClient
    {
        CookieContainer CookieContainer { get; set; }

        int? MaxRedirects { get; set; }

        string UserAgent { get; set; }

        int Timeout { get; set; }

        int ReadWriteTimeout { get; set; }

        bool UseSynchronizationContext { get; set; }

        IAuthenticator Authenticator { get; set; }

        Uri BaseUrl { get; set; }

        Encoding Encoding { get; set; }

        bool PreAuthenticate { get; set; }

        IList<Parameter> DefaultParameters { get; }

        RestRequestAsyncHandle ExecuteAsync(IRestRequest request, Action<IRestResponse, RestRequestAsyncHandle> callback);

        RestRequestAsyncHandle ExecuteAsync<T>(IRestRequest request, Action<IRestResponse<T>, RestRequestAsyncHandle> callback);

#if FRAMEWORK
        IRestResponse Execute(IRestRequest request);

        IRestResponse<T> Execute<T>(IRestRequest request) where T : new();

        byte[] DownloadData(IRestRequest request);
#endif

#if FRAMEWORK
        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        X509CertificateCollection ClientCertificates { get; set; }

        IWebProxy Proxy { get; set; }

        RequestCachePolicy CachePolicy { get; set; }

        bool Pipelined { get; set; }
#endif

        bool FollowRedirects { get; set; }

        Uri BuildUri(IRestRequest request);

#if NET45
        /// <summary>
        /// Callback function for handling the validation of remote certificates. Useful for certificate pinning and
        /// overriding certificate errors in the scope of a request.
        /// </summary>
        RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }
#endif

        /// <summary>
        /// Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        RestRequestAsyncHandle ExecuteAsyncGet(IRestRequest request, Action<IRestResponse,
            RestRequestAsyncHandle> callback, string httpMethod);

        /// <summary>
        /// Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        RestRequestAsyncHandle ExecuteAsyncPost(IRestRequest request, Action<IRestResponse,
            RestRequestAsyncHandle> callback, string httpMethod);

        /// <summary>
        /// Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        RestRequestAsyncHandle ExecuteAsyncGet<T>(IRestRequest request, Action<IRestResponse<T>,
            RestRequestAsyncHandle> callback, string httpMethod);

        /// <summary>
        /// Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        RestRequestAsyncHandle ExecuteAsyncPost<T>(IRestRequest request, Action<IRestResponse<T>,
            RestRequestAsyncHandle> callback, string httpMethod);

        void AddHandler(string contentType, IDeserializer deserializer);

        void RemoveHandler(string contentType);

        void ClearHandlers();

#if FRAMEWORK
        IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod);

        IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod);

        IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod) where T : new();

        IRestResponse<T> ExecuteAsPost<T>(IRestRequest request, string httpMethod) where T : new();
#endif

#if NET4 || MONODROID || MONOTOUCH || WP8 || WINDOWS_UWP
        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request);

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request);

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse> ExecuteTaskAsync(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse> ExecuteTaskAsync(IRestRequest request);

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request);

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request, CancellationToken token);

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request);

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request, CancellationToken token);
#endif
    }
}
