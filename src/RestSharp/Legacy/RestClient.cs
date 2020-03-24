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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Validation;

// ReSharper disable CheckNamespace

namespace RestSharp
{
    public partial class RestClient
    {
        static HttpWebRequest DoAsGetAsync(IHttp http, Action<HttpResponse> responseCb, string method) => http.AsGetAsync(responseCb, method);

        static HttpWebRequest DoAsPostAsync(IHttp http, Action<HttpResponse> responseCb, string method) => http.AsPostAsync(responseCb, method);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        /// <param name="httpMethod">Override the request method</param>
        [Obsolete("Use ExecuteAsync instead")]
        public virtual Task<IRestResponse> ExecuteTaskAsync(
            IRestRequest request,
            CancellationToken token,
            Method httpMethod
        )
            => ExecuteAsync(request, httpMethod, token);

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">HTTP call method (GET, PUT, etc)</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            Method httpMethod
        )
        {
            var method = Enum.GetName(typeof(Method), httpMethod);

            return httpMethod switch
            {
                Method.COPY  => ExecuteAsync(request, callback, method, DoAsPostAsync),
                Method.MERGE => ExecuteAsync(request, callback, method, DoAsPostAsync),
                Method.PATCH => ExecuteAsync(request, callback, method, DoAsPostAsync),
                Method.POST  => ExecuteAsync(request, callback, method, DoAsPostAsync),
                Method.PUT   => ExecuteAsync(request, callback, method, DoAsPostAsync),
                _            => ExecuteAsync(request, callback, method, DoAsGetAsync)
            };
        }

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => ExecuteAsync(request, callback, request.Method);

        /// <summary>
        ///     Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsyncGet(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod
        )
            => ExecuteAsync(request, callback, httpMethod, DoAsGetAsync);

        /// <summary>
        ///     Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle.</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsyncPost(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod
        )
        {
            request.Method = Method.POST; // Required by RestClient.BuildUri... 
            return ExecuteAsync(request, callback, httpMethod, DoAsPostAsync);
        }

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">Override the request http method</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsync<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            Method httpMethod
        )
        {
            Ensure.NotNull(request, nameof(request));
            Ensure.NotNull(callback, nameof(callback));

            request.Method = httpMethod;
            return ExecuteAsync(request, callback);
        }

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsync<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        )
            => ExecuteAsync(
                request,
                (response, asyncHandle) => DeserializeResponse(request, callback, response, asyncHandle)
            );

        /// <summary>
        ///     Executes a GET-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsyncGet<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            string httpMethod
        )
            => ExecuteAsyncGet(
                request,
                (response, asyncHandle) => DeserializeResponse(request, callback, response, asyncHandle), httpMethod
            );

        /// <summary>
        ///     Executes a POST-style request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        /// <param name="httpMethod">The HTTP method to execute</param>
        [Obsolete("This method will be removed soon in favour of the proper async call")]
        public virtual RestRequestAsyncHandle ExecuteAsyncPost<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            string httpMethod
        )
            => ExecuteAsyncPost(
                request,
                (response, asyncHandle) => DeserializeResponse(request, callback, response, asyncHandle), httpMethod
            );

        /// <summary>
        ///     Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("This method will be renamed to ExecuteGetAsync soon")]
        public virtual Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request) => ExecuteGetAsync<T>(request, CancellationToken.None);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteAsync instead")]
        public virtual Task<IRestResponse> ExecuteTaskAsync(IRestRequest request, CancellationToken token) => ExecuteAsync(request, token);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        /// <param name="httpMethod">Override the request method</param>
        [Obsolete("Use ExecuteAsync instead")]
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(
            IRestRequest request,
            CancellationToken token,
            Method httpMethod
        )
            => ExecuteAsync<T>(request, httpMethod, token);

        /// <summary>
        ///     Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        public virtual Task<IRestResponse<T>> ExecuteGetTaskAsync<T>(IRestRequest request, CancellationToken token)
            => ExecuteTaskAsync<T>(request, token, Method.GET);

        /// <summary>
        ///     Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecutePostAsync instead")]
        public virtual Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request, CancellationToken token)
            => ExecuteAsync<T>(request, Method.POST, token);

        /// <summary>
        ///     Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecutePostAsync instead")]
        public virtual Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request) => ExecutePostAsync<T>(request, CancellationToken.None);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        [Obsolete("Please use ExecuteAsync instead")]
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, Method httpMethod)
            => ExecuteAsync<T>(request, httpMethod, CancellationToken.None);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Please use ExecuteAsync instead")]
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request) => ExecuteAsync<T>(request);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Please use ExecuteAsync instead")]
        public virtual Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request, CancellationToken token) => ExecuteAsync<T>(request, token);

        /// <summary>
        ///     Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("This method will be renamed to ExecutePostAsync soon")]
        public virtual Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request, CancellationToken token) => ExecutePostAsync(request, token);

        /// <summary>
        ///     Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecutePostAsync instead")]
        public virtual Task<IRestResponse> ExecutePostTaskAsync(IRestRequest request) => ExecutePostAsync(request, CancellationToken.None);

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecuteAsync instead")]
        public virtual Task<IRestResponse> ExecuteTaskAsync(IRestRequest request) => ExecuteAsync(request, CancellationToken.None);

        /// <summary>
        ///     Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        public virtual Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request) => ExecuteGetAsync(request, CancellationToken.None);

        /// <summary>
        ///     Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="token">The cancellation token</param>
        [Obsolete("Use ExecuteGetAsync instead")]
        public virtual Task<IRestResponse> ExecuteGetTaskAsync(IRestRequest request, CancellationToken token)
            => ExecuteGetAsync(request, token);
    }
}