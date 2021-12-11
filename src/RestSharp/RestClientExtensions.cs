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

using System.Net;

namespace RestSharp; 

public static class RestClientExtensions {
    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteGetAsync<T>(request, cancellationToken);
        ThrowIfError(response);
        return response.Data;
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
    public static async Task<T?> PostAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecutePostAsync<T>(request, cancellationToken);
        ThrowIfError(response);
        return response.Data;
    }

    public static Task<RestResponse> PostAsync(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        return client.ExecuteAsync(request, Method.Post, cancellationToken);
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
    public static async Task<T?> PutAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Put, cancellationToken);
        ThrowIfError(response);
        return response.Data;
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
    public static async Task<T?> HeadAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Head, cancellationToken);
        ThrowIfError(response);
        return response.Data;
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
    public static async Task<T?> OptionsAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Options, cancellationToken);
        ThrowIfError(response);
        return response.Data;
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
    public static async Task<T?> PatchAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Patch, cancellationToken);
        ThrowIfError(response);
        return response.Data;
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
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default) {
        var response = await client.ExecuteAsync<T>(request, Method.Delete, cancellationToken);
        ThrowIfError(response);
        return response.Data;
    }

    /// <summary>
    /// Add a parameter to use on every request made with this client instance
    /// </summary>
    /// <param name="options">The RestClientOptions instance</param>
    /// <param name="p">Parameter to add</param>
    /// <returns></returns>
    public static RestClientOptions AddDefaultParameter(this RestClientOptions options, Parameter p) {
        if (p.Type == ParameterType.RequestBody)
            throw new NotSupportedException(
                "Cannot set request body from default headers. Use Request.AddBody() instead."
            );

        options.DefaultParameters.Add(p);

        return options;
    }

    /// <summary>
    /// Adds a default HTTP parameter (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <returns>This request</returns>
    public static RestClientOptions AddDefaultParameter(this RestClientOptions options, string name, object value)
        => options.AddDefaultParameter(new Parameter(name, value, ParameterType.GetOrPost));

    /// <summary>
    /// Adds a default parameter to the client options. There are four types of parameters:
    /// - GetOrPost: Either a QueryString value or encoded form value based on method
    /// - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
    /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
    /// - RequestBody: Used by AddBody() (not recommended to use directly)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">The type of parameter to add</param>
    /// <returns>This request</returns>
    public static RestClientOptions AddDefaultParameter(
        this RestClientOptions options,
        string           name,
        object           value,
        ParameterType    type
    )
        => options.AddDefaultParameter(new Parameter(name, value, type));

    /// <summary>
    /// Adds a default header to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the header to add</param>
    /// <param name="value">Value of the header to add</param>
    /// <returns></returns>
    public static RestClientOptions AddDefaultHeader(this RestClientOptions options, string name, string value)
        => options.AddDefaultParameter(name, value, ParameterType.HttpHeader);

    /// <summary>
    /// Adds default headers to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="headers">Dictionary containing the Names and Values of the headers to add</param>
    /// <returns></returns>
    public static RestClientOptions AddDefaultHeaders(this RestClientOptions options, Dictionary<string, string> headers) {
        foreach (var header in headers)
            options.AddDefaultParameter(new Parameter(header.Key, header.Value, ParameterType.HttpHeader));

        return options;
    }

    /// <summary>
    /// Adds a default URL segment parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the segment to add</param>
    /// <param name="value">Value of the segment to add</param>
    /// <returns></returns>
    public static RestClientOptions AddDefaultUrlSegment(this RestClientOptions options, string name, string value)
        => options.AddDefaultParameter(name, value, ParameterType.UrlSegment);

    /// <summary>
    /// Adds a default URL query parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="options"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the query parameter to add</param>
    /// <param name="value">Value of the query parameter to add</param>
    /// <returns></returns>
    public static RestClientOptions AddDefaultQueryParameter(this RestClientOptions options, string name, string value)
        => options.AddDefaultParameter(name, value, ParameterType.QueryString);

    static void ThrowIfError(RestResponse response) {
        var exception = response.ResponseStatus switch {
            ResponseStatus.Aborted   => new WebException("Request aborted", response.ErrorException),
            ResponseStatus.Error     => response.ErrorException,
            ResponseStatus.TimedOut  => new TimeoutException("Request timed out", response.ErrorException),
            ResponseStatus.None      => null,
            ResponseStatus.Completed => null,
            _                        => throw response.ErrorException ?? new ArgumentOutOfRangeException()
        };

        if (exception != null)
            throw exception;
    }

    /// <summary>
    /// Sets the <see cref="RestClient"/> to only use JSON
    /// </summary>
    /// <param name="client">The client instance</param>
    /// <returns></returns>
    public static RestClient UseJson(this RestClient client) {
        client.Serializers.Remove(DataFormat.Xml);
        return client;
    }

    /// <summary>
    /// Sets the <see cref="RestClient"/> to only use XML
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static RestClient UseXml(this RestClient client) {
        client.Serializers.Remove(DataFormat.Json);

        return client;
    }
}