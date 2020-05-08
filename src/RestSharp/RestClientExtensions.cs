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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Serialization;

namespace RestSharp
{
    public static partial class RestClientExtensions
    {
        /// <summary>
        ///     Execute the request and returns a response with the dynamic object as Data
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse<dynamic> ExecuteDynamic(this IRestClient client, IRestRequest request)
        {
            var     response = client.Execute<dynamic>(request);
            var     generic  = (RestResponse<dynamic>) response;
            dynamic content  = client.Deserialize<object>(response);

            generic.Data = content;

            return generic;
        }

        /// <summary>
        ///     Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteGetAsync<T>(request, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecutePostAsync<T>(request, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using PUT HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> PutAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteAsync<T>(request, Method.PUT, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> HeadAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteAsync<T>(request, Method.HEAD, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using OPTIONS HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> OptionsAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteAsync<T>(request, Method.OPTIONS, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using PATCH HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> PatchAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteAsync<T>(request, Method.PATCH, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using DELETE HTTP method. Exception will be thrown if the request does not succeed.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static async Task<T> DeleteAsync<T>(this IRestClient client, IRestRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.ExecuteAsync<T>(request, Method.DELETE, cancellationToken);
            ThrowIfError(response);
            return response.Data;
        }

        /// <summary>
        ///     Execute the request using GET HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Get<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.GET);

        /// <summary>
        ///     Execute the request using POST HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Post<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.POST);

        /// <summary>
        ///     Execute the request using PUT HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Put<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.PUT);

        /// <summary>
        ///     Execute the request using HEAD HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Head<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.HEAD);

        /// <summary>
        ///     Execute the request using OPTIONS HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Options<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.OPTIONS);

        /// <summary>
        ///     Execute the request using PATCH HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Patch<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.PATCH);

        /// <summary>
        ///     Execute the request using DELETE HTTP method.
        ///     The response data is deserialzied to the Data property of the returned response object.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <returns></returns>
        public static IRestResponse<T> Delete<T>(this IRestClient client, IRestRequest request)
            => client.Execute<T>(request, Method.DELETE);

        /// <summary>
        ///     Execute the request using GET HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Get(this IRestClient client, IRestRequest request) => client.Execute(request, Method.GET);

        /// <summary>
        ///     Execute the request using POST HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Post(this IRestClient client, IRestRequest request) => client.Execute(request, Method.POST);

        /// <summary>
        ///     Execute the request using PUT HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Put(this IRestClient client, IRestRequest request) => client.Execute(request, Method.PUT);

        /// <summary>
        ///     Execute the request using HEAD HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Head(this IRestClient client, IRestRequest request) => client.Execute(request, Method.HEAD);

        /// <summary>
        ///     Execute the request using OPTIONS HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Options(this IRestClient client, IRestRequest request) => client.Execute(request, Method.OPTIONS);

        /// <summary>
        ///     Execute the request using PATCH HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Patch(this IRestClient client, IRestRequest request) => client.Execute(request, Method.PATCH);

        /// <summary>
        ///     Execute the request using DELETE HTTP method.
        /// </summary>
        /// <param name="client">RestClient instance</param>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public static IRestResponse Delete(this IRestClient client, IRestRequest request) => client.Execute(request, Method.DELETE);

        /// <summary>
        ///     Add a parameter to use on every request made with this client instance
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="p">Parameter to add</param>
        /// <returns></returns>
        public static IRestClient AddDefaultParameter(this IRestClient restClient, Parameter p)
        {
            if (p.Type == ParameterType.RequestBody)
                throw new NotSupportedException(
                    "Cannot set request body from default headers. Use Request.AddBody() instead."
                );

            restClient.DefaultParameters.Add(p);

            return restClient;
        }

        /// <summary>
        ///     Add a new or update an existing parameter to use on every request made with this client instance
        /// </summary>
        /// <param name="restClient"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IRestClient AddOrUpdateDefaultParameter(this IRestClient restClient, Parameter p)
        {
            var existing = restClient.DefaultParameters.FirstOrDefault(x => x.Name == p.Name);

            if (existing != null) restClient.DefaultParameters.Remove(existing);

            restClient.DefaultParameters.Add(p);

            return restClient;
        }

        /// <summary>
        ///     Removes a parameter from the default parameters that are used on every request made with this client instance
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">The name of the parameter that needs to be removed</param>
        /// <returns></returns>
        public static IRestClient RemoveDefaultParameter(this IRestClient restClient, string name)
        {
            var parameter = restClient.DefaultParameters.SingleOrDefault(
                p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            );

            if (parameter != null) restClient.DefaultParameters.Remove(parameter);

            return restClient;
        }

        /// <summary>
        ///     Adds a default HTTP parameter (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
        ///     Used on every request made by this client instance
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>This request</returns>
        public static IRestClient AddDefaultParameter(this IRestClient restClient, string name, object value)
            => restClient.AddDefaultParameter(new Parameter(name, value, ParameterType.GetOrPost));

        /// <summary>
        ///     Adds a default parameter to the request. There are four types of parameters:
        ///     - GetOrPost: Either a QueryString value or encoded form value based on method
        ///     - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        ///     - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        ///     - RequestBody: Used by AddBody() (not recommended to use directly)
        ///     Used on every request made by this client instance
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        public static IRestClient AddDefaultParameter(
            this IRestClient restClient,
            string name,
            object value,
            ParameterType type
        )
            => restClient.AddDefaultParameter(new Parameter(name, value, type));

        /// <summary>
        ///     Adds a default header to the RestClient. Used on every request made by this client instance.
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">Name of the header to add</param>
        /// <param name="value">Value of the header to add</param>
        /// <returns></returns>
        public static IRestClient AddDefaultHeader(this IRestClient restClient, string name, string value)
            => restClient.AddDefaultParameter(name, value, ParameterType.HttpHeader);

        /// <summary>
        /// Adds default headers to the RestClient. Used on every request made by this client instance.
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="headers">Dictionary containing the Names and Values of the headers to add</param>
        /// <returns></returns>
        public static IRestClient AddDefaultHeaders(this IRestClient restClient, Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                restClient.AddOrUpdateDefaultParameter(new Parameter(header.Key, header.Value, ParameterType.HttpHeader));

            return restClient;
        }

        /// <summary>
        ///     Adds a default URL segment parameter to the RestClient. Used on every request made by this client instance.
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">Name of the segment to add</param>
        /// <param name="value">Value of the segment to add</param>
        /// <returns></returns>
        public static IRestClient AddDefaultUrlSegment(this IRestClient restClient, string name, string value)
            => restClient.AddDefaultParameter(name, value, ParameterType.UrlSegment);

        /// <summary>
        ///     Adds a default URL query parameter to the RestClient. Used on every request made by this client instance.
        /// </summary>
        /// <param name="restClient">The IRestClient instance</param>
        /// <param name="name">Name of the query parameter to add</param>
        /// <param name="value">Value of the query parameter to add</param>
        /// <returns></returns>
        public static IRestClient AddDefaultQueryParameter(this IRestClient restClient, string name, string value)
            => restClient.AddDefaultParameter(name, value, ParameterType.QueryString);

        static void ThrowIfError(IRestResponse response)
        {
            var exception = response.ResponseStatus switch
            {
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
        public static RestClient UseJson(this RestClient client)
        {
            foreach (var contentType in ContentType.XmlAccept)
                client.RemoveHandler(contentType);

            client.Serializers.Remove(DataFormat.Xml);

            return client;
        }

        /// <summary>
        ///     Sets the <see cref="RestClient"/> to only use XML
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static RestClient UseXml(this RestClient client)
        {
            foreach (var contentType in ContentType.JsonAccept)
                client.RemoveHandler(contentType);

            client.Serializers.Remove(DataFormat.Json);

            return client;
        }
    }
}