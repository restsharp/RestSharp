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

namespace RestSharp
{
    public partial class RestClient
    {
        /// <summary>
        ///     Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<T>(request, Method.GET, cancellationToken);

        /// <summary>
        ///     Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task<IRestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<T>(request, Method.POST, cancellationToken);

        /// <summary>
        ///     Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse> ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync(request, Method.GET, cancellationToken);

        /// <summary>
        ///     Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse> ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync(request, Method.POST, cancellationToken);
        
        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();

            try
            {
                var async = ExecuteAsync<T>(
                    request,
                    (response, _) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            taskCompletionSource.TrySetCanceled();
                        // Don't run TrySetException, since we should set Error properties and swallow exceptions
                        // to be consistent with sync methods
                        else
                            taskCompletionSource.TrySetResult(response);
                    }
                );

                var registration =
                    cancellationToken.Register(
                        () =>
                        {
                            async.Abort();
                            taskCompletionSource.TrySetCanceled();
                        }
                    );

                taskCompletionSource.Task.ContinueWith(t => registration.Dispose(), cancellationToken);
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse> ExecuteAsync(
            IRestRequest request,
            Method httpMethod,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return ExecuteAsync(request, cancellationToken);
        }

        /// <summary>
        ///     Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<IRestResponse<T>> ExecuteAsync<T>(
            IRestRequest request,
            Method httpMethod,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return ExecuteAsync<T>(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken token = default)
        {
            Ensure.NotNull(request, nameof(request));

            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();

            try
            {
                var async = ExecuteAsync(
                    request,
                    (response, _) =>
                    {
                        if (token.IsCancellationRequested)
                            taskCompletionSource.TrySetCanceled();
                        // Don't run TrySetException, since we should set Error
                        // properties and swallow exceptions to be consistent
                        // with sync methods
                        else
                            taskCompletionSource.TrySetResult(response);
                    }
                );

                var registration =
                    token.Register(
                        () =>
                        {
                            async.Abort();
                            taskCompletionSource.TrySetCanceled();
                        }
                    );

                taskCompletionSource.Task.ContinueWith(t => registration.Dispose(), token);
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }

        RestRequestAsyncHandle ExecuteAsync(
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback,
            string httpMethod,
            Func<IHttp, Action<HttpResponse>, string, HttpWebRequest> getWebRequest
        )
        {
            request.SerializeRequestBody(Serializers, request.XmlSerializer, request.JsonSerializer);

            AuthenticateIfNeeded(request);

            var http = ConfigureHttp(request);

            request.OnBeforeRequest?.Invoke(http);

            var asyncHandle = new RestRequestAsyncHandle();

            Action<HttpResponse> responseCb = ProcessResponse;

            if (UseSynchronizationContext && SynchronizationContext.Current != null)
            {
                var ctx = SynchronizationContext.Current;
                var cb  = responseCb;

                responseCb = resp => ctx.Post(s => cb(resp), null);
            }

            asyncHandle.WebRequest = getWebRequest(http, responseCb, httpMethod);

            return asyncHandle;

            void ProcessResponse(IHttpResponse httpResponse)
            {
                var restResponse = RestResponse.FromHttpResponse(httpResponse, request);
                restResponse.Request.IncreaseNumAttempts();
                callback(restResponse, asyncHandle);
            }
        }

        void DeserializeResponse<T>(
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
            IRestResponse response,
            RestRequestAsyncHandle asyncHandle
        )
            => callback(Deserialize<T>(request, response), asyncHandle);
    }
}