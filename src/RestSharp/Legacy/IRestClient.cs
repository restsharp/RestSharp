//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Deserializers;
using RestSharp.Serialization;

// ReSharper disable CheckNamespace

namespace RestSharp
{
    public partial interface IRestClient
    {
        [Obsolete("Use the overload that accepts the delegate factory")]
        IRestClient UseSerializer(IRestSerializer serializer);

        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        );

        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsync<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        );

        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            Method httpMethod
        );

        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsync<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            Method httpMethod
        );

        /// <summary>
        ///     Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsyncGet(
            IRestRequest request,
            Action<IRestResponse,
                RestRequestAsyncHandle> callback,
            string httpMethod
        );

        /// <summary>
        ///     Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsyncPost(
            IRestRequest request,
            Action<IRestResponse,
                RestRequestAsyncHandle> callback,
            string httpMethod
        );

        /// <summary>
        ///     Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsyncGet<T>(
            IRestRequest request,
            Action<IRestResponse<T>,
                RestRequestAsyncHandle> callback,
            string httpMethod
        );

        /// <summary>
        ///     Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        RestRequestAsyncHandle ExecuteAsyncPost<T>(
            IRestRequest request,
            Action<IRestResponse<T>,
                RestRequestAsyncHandle> callback,
            string httpMethod
        );

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("This method will be renamed to ExecuteAsync soon")]
        Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request);

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("UseExecuteAsync instead")]
        Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        [Obsolete("Use ExecuteAsync instead")]
        Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, Method httpMethod);

        /// <summary>
        ///     Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request);

        /// <summary>
        ///     Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecutePostAsync instead")]
        Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request);

        /// <summary>
        ///     Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecutePostAsync instead")]
        Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteAsync instead")]
        Task<IRestResponse> ExecuteTaskAsync(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        /// <param name="httpMethod">Override the request method</param>
        [Obsolete("Use ExecuteAsync instead")]
        Task<IRestResponse> ExecuteTaskAsync(IRestRequest request, CancellationToken token, Method httpMethod);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecuteAsync instead")]
        Task<IRestResponse> ExecuteTaskAsync(IRestRequest request);

        /// <summary>
        ///     Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request);

        /// <summary>
        ///     Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecutePostAsync instead")]
        Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request);

        /// <summary>
        ///     Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecutePostAsync instead")]
        Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request, CancellationToken token);

        /// <summary>
        ///     Adds or replaces a deserializer for the specified content type
        /// </summary>
        /// <param name="contentType">Content type for which the deserializer will be replaced</param>
        /// <param name="deserializer">Custom deserializer</param>
        [Obsolete("Use the overload that accepts a factory delegate")]
        void AddHandler(string contentType, IDeserializer deserializer);
    }
}