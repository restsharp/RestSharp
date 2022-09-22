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
using RestSharp.Serializers;

namespace RestSharp;

[PublicAPI]
public static partial class RestClientExtensions {
    /// <summary>
    /// Executes a GET-style request asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecuteGetAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync<T>(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a GET-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecuteGetAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a POST-style request asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecutePostAsync<T>(
        this RestClient   client,
        RestRequest       request,
        CancellationToken cancellationToken = default
    )
        => client.ExecuteAsync<T>(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a POST-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecutePostAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a PUT-style request asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecutePutAsync<T>(
        this RestClient   client,
        RestRequest       request,
        CancellationToken cancellationToken = default
    )
        => client.ExecuteAsync<T>(request, Method.Put, cancellationToken);

    /// <summary>
    /// Executes a PUP-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecutePutAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(request, Method.Put, cancellationToken);

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task<RestResponse<T>> ExecuteAsync<T>(
        this RestClient   client,
        RestRequest       request,
        CancellationToken cancellationToken = default
    ) {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var response = await client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        return client.Deserialize<T>(request, response);
    }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecuteAsync(
        this RestClient   client,
        RestRequest       request,
        Method            httpMethod,
        CancellationToken cancellationToken = default
    ) {
        Ensure.NotNull(request, nameof(request));

        request.Method = httpMethod;
        return client.ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse<T>> ExecuteAsync<T>(
        this RestClient   client,
        RestRequest       request,
        Method            httpMethod,
        CancellationToken cancellationToken = default
    ) {
        Ensure.NotNull(request, nameof(request));

        request.Method = httpMethod;
        return client.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteGetAsync<T>(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }
    
    public static async Task<RestResponse> GetAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteGetAsync(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> PostAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecutePostAsync<T>(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static RestResponse Post(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PostAsync(request));

    public static async Task<RestResponse> PostAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecutePostAsync(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using PUT HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> PutAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Put, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static async Task<RestResponse> PutAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync(request, Method.Put, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> HeadAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Head, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static async Task<RestResponse> HeadAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync(request, Method.Head, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using OPTIONS HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> OptionsAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Options, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static async Task<RestResponse> OptionsAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync(request, Method.Options, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using PATCH HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> PatchAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Patch, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static async Task<RestResponse> PatchAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync(request, Method.Patch, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using DELETE HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> DeleteAsync<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Delete, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    public static async Task<RestResponse> DeleteAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync(request, Method.Delete, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// A specialized method to download files.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The downloaded file.</returns>
    [PublicAPI]
    public static async Task<byte[]?> DownloadDataAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default) {
#if NETSTANDARD
        using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#else
        await using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#endif
        return stream == null ? null : await stream.ReadAsBytes(cancellationToken).ConfigureAwait(false);
    }

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
        this RestClient                            client,
        string                                     resource,
        [EnumeratorCancellation] CancellationToken cancellationToken
    ) {
        var request = new RestRequest(resource);

#if NETSTANDARD
        using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#else
        await using var stream = await client.DownloadStreamAsync(request, cancellationToken).ConfigureAwait(false);
#endif
        if (stream == null) yield break;

        var serializer = client.Serializers[DataFormat.Json].GetSerializer();

        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested) {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line)) continue;

            var response = new RestResponse { Content = line };
            yield return serializer.Deserializer.Deserialize<T>(response)!;
        }
    }

    /// <summary>
    /// Sets the <see cref="RestClient"/> to only use JSON
    /// </summary>
    /// <param name="client">Client instance to work with</param>
    /// <returns>Reference to the client instance</returns>
    public static RestClient UseJson(this RestClient client) {
        client.Serializers.Remove(DataFormat.Xml);
        client.AssignAcceptedContentTypes();
        return client;
    }

    /// <summary>
    /// Sets the <see cref="RestClient"/> to only use XML
    /// </summary>
    /// <param name="client">Client instance to work with</param>
    /// <returns>Reference to the client instance</returns>
    public static RestClient UseXml(this RestClient client) {
        client.Serializers.Remove(DataFormat.Json);
        client.AssignAcceptedContentTypes();
        return client;
    }

    /// <summary>
    /// Sets the <see cref="RestClient"/> to only use the passed in custom serializer
    /// </summary>
    /// <param name="client">Client instance to work with</param>
    /// <param name="serializerFactory">Function that returns the serializer instance</param>
    /// <returns>Reference to the client instance</returns>
    public static RestClient UseOnlySerializer(this RestClient client, Func<IRestSerializer> serializerFactory) {
        client.Serializers.Clear();
        client.UseSerializer(serializerFactory);
        return client;
    }
}