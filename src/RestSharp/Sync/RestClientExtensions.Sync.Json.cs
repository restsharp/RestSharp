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
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? GetJson<TResponse>(
        this RestClient   client,
        string            resource)
        => AsyncHelpers.RunSync(() => client.GetJsonAsync<TResponse>(resource));

    /// <summary>
    /// Calls the URL specified in the <code>resource</code> parameter, expecting a JSON response back. Deserializes and returns the response.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="parameters">Parameters to pass to the request</param>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? GetJson<TResponse>(
        this RestClient   client,
        string            resource,
        object            parameters)
        => AsyncHelpers.RunSync(() => client.GetJsonAsync<TResponse>(resource, parameters));

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
    public static TResponse? PostJson<TRequest, TResponse>(
        this RestClient   client,
        string            resource,
        TRequest          request
    ) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PostJsonAsync<TRequest, TResponse>(resource, request));

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a POST call to the resource specified in the <code>resource</code> parameter.
    /// Expects no response back, just the status code.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <returns>Response status code</returns>
    public static HttpStatusCode PostJson<TRequest>(
        this RestClient client,
        string          resource,
        TRequest        request
    ) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PostJsonAsync(resource, request));

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a PUT call to the resource specified in the <code>resource</code> parameter.
    /// Expects a JSON response back, deserializes it to <code>TResponse</code> type and returns it.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <typeparam name="TResponse">Response object type</typeparam>
    /// <returns>Deserialized response object</returns>
    public static TResponse? PutJson<TRequest, TResponse>(
        this RestClient   client,
        string            resource,
        TRequest          request
    ) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PutJsonAsync<TRequest, TResponse>(resource, request));

    /// <summary>
    /// Serializes the <code>request</code> object to JSON and makes a PUT call to the resource specified in the <code>resource</code> parameter.
    /// Expects no response back, just the status code.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="resource">Resource URL</param>
    /// <param name="request">Request object, must be serializable to JSON</param>
    /// <typeparam name="TRequest">Request object type</typeparam>
    /// <returns>Response status code</returns>
    public static HttpStatusCode PutJson<TRequest>(
        this RestClient client,
        string          resource,
        TRequest        request
    ) where TRequest : class
        => AsyncHelpers.RunSync(() => client.PutJsonAsync(resource, request));
}