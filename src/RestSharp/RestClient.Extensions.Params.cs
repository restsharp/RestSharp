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
    /// Add a parameter to use on every request made with this client instance
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="parameter"><see cref="Parameter"/> to add</param>
    /// <returns></returns>
    public static IRestClient AddDefaultParameter(this IRestClient client, Parameter parameter) {
        client.DefaultParameters.AddParameter(parameter);
        return client;
    }

    /// <summary>
    /// Adds a default HTTP parameter (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <returns>This request</returns>
    public static IRestClient AddDefaultParameter(this IRestClient client, string name, string value)
        => client.AddDefaultParameter(new GetOrPostParameter(name, value));

    /// <summary>
    /// Adds a default parameter to the client options. There are four types of parameters:
    /// - GetOrPost: Either a QueryString value or encoded form value based on method
    /// - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
    /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
    /// - RequestBody: Used by AddBody() (not recommended to use directly)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">The type of parameter to add</param>
    /// <returns>This request</returns>
    public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value, ParameterType type)
        => client.AddDefaultParameter(Parameter.CreateParameter(name, value, type));

    /// <summary>
    /// Adds a default header to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="name">Name of the header to add</param>
    /// <param name="value">Value of the header to add</param>
    /// <returns></returns>
    public static IRestClient AddDefaultHeader(this IRestClient client, string name, string value)
        => client.AddDefaultParameter(new HeaderParameter(name, value));

    /// <summary>
    /// Adds default headers to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="headers">Dictionary containing the Names and Values of the headers to add</param>
    /// <returns></returns>
    public static IRestClient AddDefaultHeaders(this IRestClient client, Dictionary<string, string> headers) {
        foreach (var header in headers) client.AddDefaultParameter(new HeaderParameter(header.Key, header.Value));

        return client;
    }

    /// <summary>
    /// Adds a default URL segment parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="name">Name of the segment to add</param>
    /// <param name="value">Value of the segment to add</param>
    /// <returns></returns>
    public static IRestClient AddDefaultUrlSegment(this IRestClient client, string name, string value)
        => client.AddDefaultParameter(new UrlSegmentParameter(name, value));

    /// <summary>
    /// Adds a default URL query parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClient"/> instance</param>
    /// <param name="name">Name of the query parameter to add</param>
    /// <param name="value">Value of the query parameter to add</param>
    /// <returns></returns>
    public static IRestClient AddDefaultQueryParameter(this IRestClient client, string name, string value)
        => client.AddDefaultParameter(new QueryParameter(name, value));
}
