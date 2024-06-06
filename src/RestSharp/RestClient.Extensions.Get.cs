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
    /// <summary>
    /// Executes a GET-style asynchronously, authenticating if needed.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecuteGetAsync(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a GET-style asynchronously, authenticating if needed.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="resource">Request resource</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecuteGetAsync(this IRestClient client, string resource, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(new RestRequest(resource), Method.Get, cancellationToken);

    /// <summary>
    /// Executes a GET-style synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse ExecuteGet(this IRestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Get));

    /// <summary>
    /// Executes a GET-style request asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecuteGetAsync<T>(
        this IRestClient  client,
        RestRequest       request,
        CancellationToken cancellationToken = default
    )
        => client.ExecuteAsync<T>(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a GET-style request to the specified resource URL asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="resource">Request resource</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecuteGetAsync<T>(
        this IRestClient  client,
        string            resource,
        CancellationToken cancellationToken = default
    )
        => client.ExecuteAsync<T>(new RestRequest(resource), Method.Get, cancellationToken);

    /// <summary>
    /// Executes a GET-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecuteGet<T>(this IRestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Get));
    
    /// <summary>
    /// Executes a GET-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="resource">Request resource</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecuteGet<T>(this IRestClient client, string resource)
        => AsyncHelpers.RunSync(() => client.ExecuteGetAsync<T>(resource));

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public static async Task<RestResponse> GetAsync(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteGetAsync(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <returns></returns>
    public static RestResponse Get(this IRestClient client, RestRequest request) => AsyncHelpers.RunSync(() => client.GetAsync(request));

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteGetAsync<T>(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Get<T>(this IRestClient client, RestRequest request) => AsyncHelpers.RunSync(() => client.GetAsync<T>(request));

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns></returns>
    public static Task<TResponse?> GetAsync<TResponse>(this IRestClient client, string resource, CancellationToken cancellationToken = default)
        => client.GetAsync<TResponse>(new RestRequest(resource), cancellationToken);

    [Obsolete("Use GetAsync instead")]
    public static Task<TResponse?> GetJsonAsync<TResponse>(
        this IRestClient  client,
        string            resource,
        CancellationToken cancellationToken = default
    )
        => client.GetAsync<TResponse>(resource, cancellationToken);

    [Obsolete("Use Get instead")]
    public static TResponse? GetJson<TResponse>(this IRestClient client, string resource)
        => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource));

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? Get<TResponse>(this IRestClient client, string resource)
        => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource));

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="parameters">Parameters to pass to the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static Task<TResponse?> GetAsync<TResponse>(
        this IRestClient  client,
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
    public static Task<TResponse?> GetJsonAsync<TResponse>(
        this IRestClient  client,
        string            resource,
        object            parameters,
        CancellationToken cancellationToken = default
    )
        => client.GetAsync<TResponse>(resource, parameters, cancellationToken);

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="parameters">Parameters to pass to the request</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? Get<TResponse>(this IRestClient client, string resource, object parameters)
        => AsyncHelpers.RunSync(() => client.GetAsync<TResponse>(resource, parameters));

    [Obsolete("Use Get instead")]
    public static TResponse? GetJson<TResponse>(this IRestClient client, string resource, object parameters)
        => client.Get<TResponse>(resource, parameters);
}