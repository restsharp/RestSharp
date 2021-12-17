//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
    /// Adds a default HTTP parameter (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <returns>This request</returns>
    public static RestClient AddDefaultParameter(this RestClient client, string name, object value)
        => client.AddDefaultParameter(new Parameter(name, value, ParameterType.GetOrPost));

    /// <summary>
    /// Adds a default parameter to the client options. There are four types of parameters:
    /// - GetOrPost: Either a QueryString value or encoded form value based on method
    /// - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
    /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
    /// - RequestBody: Used by AddBody() (not recommended to use directly)
    /// Used on every request made by this client instance
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">The type of parameter to add</param>
    /// <returns>This request</returns>
    public static RestClient AddDefaultParameter(
        this RestClient client,
        string          name,
        object          value,
        ParameterType   type
    )
        => client.AddDefaultParameter(new Parameter(name, value, type));

    /// <summary>
    /// Adds a default header to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the header to add</param>
    /// <param name="value">Value of the header to add</param>
    /// <returns></returns>
    public static RestClient AddDefaultHeader(this RestClient client, string name, string value)
        => client.AddDefaultParameter(name, value, ParameterType.HttpHeader);

    /// <summary>
    /// Adds default headers to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="headers">Dictionary containing the Names and Values of the headers to add</param>
    /// <returns></returns>
    public static RestClient AddDefaultHeaders(this RestClient client, Dictionary<string, string> headers) {
        foreach (var header in headers)
            client.AddDefaultParameter(new Parameter(header.Key, header.Value, ParameterType.HttpHeader));

        return client;
    }

    /// <summary>
    /// Adds a default URL segment parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the segment to add</param>
    /// <param name="value">Value of the segment to add</param>
    /// <returns></returns>
    public static RestClient AddDefaultUrlSegment(this RestClient client, string name, string value)
        => client.AddDefaultParameter(name, value, ParameterType.UrlSegment);

    /// <summary>
    /// Adds a default URL query parameter to the RestClient. Used on every request made by this client instance.
    /// </summary>
    /// <param name="client"><see cref="RestClientOptions"/> instance</param>
    /// <param name="name">Name of the query parameter to add</param>
    /// <param name="value">Value of the query parameter to add</param>
    /// <returns></returns>
    public static RestClient AddDefaultQueryParameter(this RestClient client, string name, string value)
        => client.AddDefaultParameter(name, value, ParameterType.QueryString);
}