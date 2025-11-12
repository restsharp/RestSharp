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
        /// Executes a HEAD-style request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<RestResponse> ExecuteHeadAsync(RestRequest request, CancellationToken cancellationToken = default)
            => client.ExecuteAsync(request, Method.Head, cancellationToken);

        /// <summary>
        /// Executes a HEAD-style synchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public RestResponse ExecuteHead(RestRequest request)
            => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Head));

        /// <summary>
        /// Executes a HEAD-style request asynchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Deserialized response content</returns>
        public Task<RestResponse<T>> ExecuteHeadAsync<T>(
            RestRequest       request,
            CancellationToken cancellationToken = default
        )
            => client.ExecuteAsync<T>(request, Method.Head, cancellationToken);

        /// <summary>
        /// Executes a HEAD-style request synchronously, authenticating if needed.
        /// The response content then gets deserialized to T.
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <returns>Deserialized response content</returns>
        public RestResponse<T> ExecuteHead<T>(RestRequest request)
            => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Head));

        /// <summary>
        /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
        /// The response data is deserialized to the Data property of the returned response object.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public async Task<T?> HeadAsync<T>(RestRequest request, CancellationToken cancellationToken = default) {
            var response = await client.ExecuteAsync<T>(request, Method.Head, cancellationToken).ConfigureAwait(false);
            return response.ThrowIfError().Data;
        }

        /// <summary>
        /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
        /// The response data is deserialized to the Data property of the returned response object.
        /// </summary>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public T? Head<T>(RestRequest request) => AsyncHelpers.RunSync(() => client.HeadAsync<T>(request));

        /// <summary>
        /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<RestResponse> HeadAsync(RestRequest request, CancellationToken cancellationToken = default) {
            var response = await client.ExecuteAsync(request, Method.Head, cancellationToken).ConfigureAwait(false);
            return response.ThrowIfError();
        }

        /// <summary>
        /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public RestResponse Head(RestRequest request) => AsyncHelpers.RunSync(() => client.HeadAsync(request));
    }
}
