//   Copyright (c) .NET Foundation and Contributors
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

using System.Runtime.CompilerServices;
using RestSharp.Extensions;

namespace RestSharp;

[PublicAPI]
public static partial class RestClientExtensions {
    /// <param name="client"></param>
    extension(IRestClient client) {
        [PublicAPI]
        [Obsolete("Please use the async overload with a cancellation token")]
        public RestResponse<T> Deserialize<T>(RestResponse response)
            => AsyncHelpers.RunSync(()
                => client.Serializers.Deserialize<T>(response.Request, response, client.Options, CancellationToken.None).AsTask()
            );

        [PublicAPI]
        public ValueTask<RestResponse<T>> Deserialize<T>(RestResponse response, CancellationToken cancellationToken)
            => client.Serializers.Deserialize<T>(response.Request, response, client.Options, cancellationToken);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task<RestResponse<T>> ExecuteAsync<T>(
            RestRequest       request,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request, nameof(request));

            var response = await client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            return await client.Serializers.Deserialize<T>(request, response, client.Options, cancellationToken);
        }

        /// <summary>
        /// Executes the request synchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        public RestResponse<T> Execute<T>(RestRequest request) => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request));

        /// <summary>
        /// Executes the request synchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        public RestResponse Execute(RestRequest request) => AsyncHelpers.RunSync(() => client.ExecuteAsync(request));

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<RestResponse> ExecuteAsync(
            RestRequest       request,
            Method            httpMethod,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return client.ExecuteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Executes the request synchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        public RestResponse Execute(RestRequest request, Method httpMethod) => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, httpMethod));

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<RestResponse<T>> ExecuteAsync<T>(
            RestRequest       request,
            Method            httpMethod,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return client.ExecuteAsync<T>(request, cancellationToken);
        }

        /// <summary>
        /// Executes the request synchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        public RestResponse<T> Execute<T>(RestRequest request, Method httpMethod)
            => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, httpMethod));

        /// <summary>
        /// A specialized method to download files.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The downloaded file.</returns>
        [PublicAPI]
        public async Task<byte[]?> DownloadDataAsync(RestRequest request, CancellationToken cancellationToken = default) {
#if NET
            await using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#else
            using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#endif
            return stream == null ? null : await stream.ReadAsBytes(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A specialized method to download files as streams.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The downloaded stream.</returns>
        [PublicAPI]
        public Stream? DownloadStream(RestRequest request, CancellationToken cancellationToken = default)
            => AsyncHelpers.RunSync(() => client.DownloadStreamAsync(request, cancellationToken));

        /// <summary>
        /// A specialized method to download files.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <returns>The downloaded file.</returns>
        public byte[]? DownloadData(RestRequest request) => AsyncHelpers.RunSync(() => client.DownloadDataAsync(request));

        /// <summary>
        /// Reads a stream returned by the specified endpoint, deserializes each line to JSON and returns each object asynchronously.
        /// It is required for each JSON object to be returned in a single line.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        public async IAsyncEnumerable<T> StreamJsonAsync<T>(
            string                                     resource,
            [EnumeratorCancellation] CancellationToken cancellationToken
        ) {
            var request = new RestRequest(resource);

#if NET
        await using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#else
            using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#endif
            if (stream == null) yield break;

            var serializer = client.Serializers.GetSerializer(DataFormat.Json);

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested) {
#if NET7_0_OR_GREATER
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
#else
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
#endif
                if (string.IsNullOrWhiteSpace(line)) continue;

                var response = new RestResponse(request) { Content = line };
                yield return serializer.Deserializer.Deserialize<T>(response)!;
            }
        }
    }
}