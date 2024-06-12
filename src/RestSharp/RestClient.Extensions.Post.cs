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
    /// Executes a POST-style request asynchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Deserialized response content</returns>
    public static Task<RestResponse<T>> ExecutePostAsync<T>(
        this IRestClient  client,
        RestRequest       request,
        CancellationToken cancellationToken = default
    )
        => client.ExecuteAsync<T>(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a POST-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecutePost<T>(this IRestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Post));

    /// <summary>
    /// Executes a POST-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static Task<RestResponse> ExecutePostAsync(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => client.ExecuteAsync(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a POST-style synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse ExecutePost(this IRestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Post));

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> PostAsync<T>(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecutePostAsync<T>(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError().Data;
    }

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Post<T>(this IRestClient client, RestRequest request) => AsyncHelpers.RunSync(() => client.PostAsync<T>(request));

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public static async Task<RestResponse> PostAsync(this IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecutePostAsync(request, cancellationToken).ConfigureAwait(false);
        return response.ThrowIfError();
    }

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <returns></returns>
    public static RestResponse Post(this IRestClient client, RestRequest request) => AsyncHelpers.RunSync(() => client.PostAsync(request));

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a POST call to the resource specified in the <code>resource</code> parameter.
    /// Expects a JSON response back, deserializes it to <code>TResponse</code> type and returns it.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
        this IRestClient  client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        return client.PostAsync<TResponse>(restRequest, cancellationToken);
    }

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a POST call to the resource specified in the <code>resource</code> parameter.
    /// Expects a JSON response back, deserializes it to <code>TResponse</code> type and returns it.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? PostJson<TRequest, TResponse>(this IRestClient client, string resource, TRequest request) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PostJsonAsync<TRequest, TResponse>(resource, request));

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a POST call to the resource specified in the <code>resource</code> parameter.
    /// Expects no response back, just the status code.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <returns>Response status code</returns>
    public static async Task<HttpStatusCode> PostJsonAsync<TRequest>(
        this IRestClient  client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        var response    = await client.PostAsync(restRequest, cancellationToken).ConfigureAwait(false);
        return response.StatusCode;
    }

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a POST call to the resource specified in the <code>resource</code> parameter.
    /// Expects no response back, just the status code.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <returns>Response status code</returns>
    public static HttpStatusCode PostJson<TRequest>(this IRestClient client, string resource, TRequest request) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PostJsonAsync(resource, request));
}
