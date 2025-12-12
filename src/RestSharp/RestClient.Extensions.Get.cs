//  Copyright (c) .NET Foundation and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

namespace RestSharp;

public static partial class RestClientExtensions {
    /// <param name="client"></param>
    extension(IRestClient client) {
        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed.
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<RestResponse> ExecuteGetAsync(RestRequest request, CancellationToken cancellationToken = default)
            => client.ExecuteAsync(request, Method.Get, cancellationToken);

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed.
        /// </summary>
        /// <param name="resource">Request resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<RestResponse> ExecuteGetAsync(string resource, CancellationToken cancellationToken = default)
            => client.ExecuteAsync(new(resource), Method.Get, cancellationToken);

        /// <summary>
        /// Executes a GET-style synchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public RestResponse ExecuteGet(RestRequest request)
            => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Get));

        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized response content</returns>
        public Task<RestResponse<T>> ExecuteGetAsync<T>(
            RestRequest       request,
            CancellationToken cancellationToken = default
        )
            => client.ExecuteAsync<T>(request, Method.Get, cancellationToken);

        /// <summary>
        /// Executes a GET-style request to the specified resource URL asynchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="resource">Request resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized response content</returns>
        public Task<RestResponse<T>> ExecuteGetAsync<T>(
            string            resource,
            CancellationToken cancellationToken = default
        )
            => client.ExecuteAsync<T>(new(resource), Method.Get, cancellationToken);

        /// <summary>
        /// Executes a GET-style request synchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <returns>Deserialized response content</returns>
        public RestResponse<T> ExecuteGet<T>(RestRequest request)
            => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Get));

        /// <summary>
        /// Executes a GET-style request synchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="resource">Request resource</param>
        /// <returns>Deserialized response content</returns>
        public RestResponse<T> ExecuteGet<T>(string resource)
            => AsyncHelpers.RunSync(() => client.ExecuteGetAsync<T>(resource));

        /// <summary>
        /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<RestResponse> GetAsync(RestRequest request, CancellationToken cancellationToken = default) {
            var response = await client.ExecuteGetAsync(request, cancellationToken).ConfigureAwait(false);
            return response.ThrowIfError();
        }

        /// <summary>
        /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public RestResponse Get(RestRequest request) => AsyncHelpers.RunSync(() => client.GetAsync(request));

        /// <summary>
        /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
        /// The response data is deserialized to the Data property of the returned response object.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(RestRequest request, CancellationToken cancellationToken = default) {
            var response = await client.ExecuteGetAsync<T>(request, cancellationToken).ConfigureAwait(false);
            return response.ThrowIfError().Data;
        }

        /// <summary>
        /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
        /// The response data is deserialized to the Data property of the returned response object.
        /// </summary>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public T? Get<T>(RestRequest request) => AsyncHelpers.RunSync(() => client.GetAsync<T>(request));

        /// <summary>
        /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
        /// </summary>
        /// <param name="resource">Resource URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TResponse">Response object type</typeparam>
        /// <returns></returns>
        public Task<TResponse?> GetAsync<TResponse>(string resource, CancellationToken cancellationToken = default)
            => client.GetAsync<TResponse>(new RestRequest(resource), cancellationToken);

        [Obsolete("Use GetAsync instead")]
        public Task<TResponse?> GetJsonAsync<TResponse>(
            string            resource,
            CancellationToken cancellationToken = default
        )
            => client.GetAsync<TResponse>(resource, cancellationToken);

        [Obsolete("Use Get instead")]
        public TResponse? GetJson<TResponse>(string resource)
            => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource));

        /// <summary>
        /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
        /// </summary>
        /// <param name="resource">Resource URL</param>
        /// <typeparam name="TResponse">Response object type</typeparam>
        /// <returns>Deserialized response object</returns>
        public TResponse? Get<TResponse>(string resource)
            => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource));

        /// <summary>
        /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
        /// </summary>
        /// <param name="resource">Resource URL</param>
        /// <param name="parameters">Parameters to pass to the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TResponse">Response object type</typeparam>
        /// <returns>Deserialized response object</returns>
        public Task<TResponse?> GetAsync<TResponse>(
            string            resource,
            object            parameters,
            CancellationToken cancellationToken = default
        ) {
            var props   = parameters.GetProperties();
            var request = new RestRequest(resource);

            foreach (var prop in props) {
                Parameter parameter = resource.Contains($"{prop.Name}")
                    ? new UrlSegmentParameter(prop.Name, prop.Value!, prop.Encode)
                    : new QueryParameter(prop.Name, prop.Value, prop.Encode);
                request.AddParameter(parameter);
            }

            return client.GetAsync<TResponse>(request, cancellationToken);
        }

        [Obsolete("Use GetAsync instead")]
        public Task<TResponse?> GetJsonAsync<TResponse>(
            string            resource,
            object            parameters,
            CancellationToken cancellationToken = default
        )
            => client.GetAsync<TResponse>(resource, parameters, cancellationToken);

        /// <summary>
        /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
        /// </summary>
        /// <param name="resource">Resource URL</param>
        /// <param name="parameters">Parameters to pass to the request</param>
        /// <typeparam name="TResponse">Response object type</typeparam>
        /// <returns>Deserialized response object</returns>
        public TResponse? Get<TResponse>(string resource, object parameters)
            => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource, parameters));

        [Obsolete("Use Get instead")]
        public TResponse? GetJson<TResponse>(string resource, object parameters)
            => client.Get<TResponse>(resource, parameters);
    }
}