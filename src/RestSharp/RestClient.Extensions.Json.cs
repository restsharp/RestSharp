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

using System.Net;
using RestSharp.Extensions;

namespace RestSharp;

public static partial class RestClientExtensions {
    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns></returns>
    public static Task<TResponse?> GetJsonAsync<TResponse>(this RestClient client, string resource, CancellationToken cancellationToken = default) {
        var request = new RestRequest(resource);
        return client.GetAsync<TResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="parameters">Parameters to pass to the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static Task<TResponse?> GetJsonAsync<TResponse>(
        this RestClient   client,
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
        this RestClient   client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        return client.PostAsync<TResponse>(restRequest, cancellationToken);
    }

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
        this RestClient   client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        var response    = await client.PostAsync(restRequest, cancellationToken).ConfigureAwait(false);
        return response.StatusCode;
    }

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a PUT call to the resource specified in the <code>resource</code> parameter.
    /// Expects a JSON response back, deserializes it to <code>TResponse</code> type and returns it.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static Task<TResponse?> PutJsonAsync<TRequest, TResponse>(
        this RestClient   client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        return client.PutAsync<TResponse>(restRequest, cancellationToken);
    }

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a PUT call to the resource specified in the <code>resource</code> parameter.
    /// Expects no response back, just the status code.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <returns>Response status code</returns>
    public static async Task<HttpStatusCode> PutJsonAsync<TRequest>(
        this RestClient   client,
        string            resource,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest(resource).AddJsonBody(request);
        var response    = await client.PutAsync(restRequest, cancellationToken).ConfigureAwait(false);
        return response.StatusCode;
    }
}
