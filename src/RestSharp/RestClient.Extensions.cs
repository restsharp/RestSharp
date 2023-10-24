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
    [PublicAPI]
    public static ValueTask<RestResponse<T>> Deserialize<T>(this IRestClient client, RestResponse response, CancellationToken cancellationToken)
        => client.Serializers.Deserialize<T>(response.Request, response, client.Options, cancellationToken);

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task<RestResponse<T>> ExecuteAsync<T>(
        this IRestClient  client,
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
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse<T> Execute<T>(this IRestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request));

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecuteAsync(
        this IRestClient  client,
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
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    public static RestResponse Execute(this IRestClient client, RestRequest request, Method httpMethod)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, httpMethod));

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse<T>> ExecuteAsync<T>(
        this IRestClient  client,
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
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    public static RestResponse<T> Execute<T>(this IRestClient client, RestRequest request, Method httpMethod)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, httpMethod));

    /// <summary>
    /// A specialized method to download files.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The downloaded file.</returns>
    [PublicAPI]
    public static async Task<byte[]?> DownloadDataAsync(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
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
    /// <param name="client"></param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The downloaded stream.</returns>
    [PublicAPI]
    public static Stream? DownloadStream(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => AsyncHelpers.RunSync(() => client.DownloadStreamAsync(request, cancellationToken));

    /// <summary>
    /// A specialized method to download files.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <returns>The downloaded file.</returns>
    public static byte[]? DownloadData(this IRestClient client, RestRequest request) => AsyncHelpers.RunSync(() => client.DownloadDataAsync(request));

    /// <summary>
    /// Reads a stream returned by the specified endpoint, deserializes each line to JSON and returns each object asynchronously.
    /// It is required for each JSON object to be returned in a single line.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [PublicAPI]
    public static async IAsyncEnumerable<T> StreamJsonAsync<T>(
        this IRestClient                           client,
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
#if NET7_0
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
