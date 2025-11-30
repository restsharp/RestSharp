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

using RestSharp.Extensions;

namespace RestSharp;

public static partial class RestRequestExtensions {
    /// <summary>
    /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT).
    /// The value will be converted to string using the culture specified in <see cref="RestClientOptions.CultureForParameters"/>.
    /// </summary>
    /// <param name="client">The RestClient instance containing the culture settings</param>
    /// <param name="request">The request to add the parameter to</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddParameter<T>(this RestRequest request, IRestClient client, string name, T value, bool encode = true) where T : struct
        => request.AddParameter(name, value.ToStringWithCulture(client.Options.CultureForParameters), encode);

    /// <summary>
    /// Adds or updates a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT).
    /// The value will be converted to string using the culture specified in <see cref="RestClientOptions.CultureForParameters"/>.
    /// </summary>
    /// <param name="client">The RestClient instance containing the culture settings</param>
    /// <param name="request">The request to add the parameter to</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddOrUpdateParameter<T>(this RestRequest request, IRestClient client, string name, T value, bool encode = true) where T : struct
        => request.AddOrUpdateParameter(name, value.ToStringWithCulture(client.Options.CultureForParameters), encode);

    /// <summary>
    /// Adds a query string parameter to the request.
    /// The value will be converted to string using the culture specified in <see cref="RestClientOptions.CultureForParameters"/>.
    /// </summary>
    /// <param name="client">The RestClient instance containing the culture settings</param>
    /// <param name="request">The request to add the parameter to</param>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddQueryParameter<T>(this RestRequest request, IRestClient client, string name, T value, bool encode = true) where T : struct
        => request.AddQueryParameter(name, value.ToStringWithCulture(client.Options.CultureForParameters), encode);

    /// <summary>
    /// Adds a URL segment parameter to the request.
    /// The value will be converted to string using the culture specified in <see cref="RestClientOptions.CultureForParameters"/>.
    /// </summary>
    /// <param name="client">The RestClient instance containing the culture settings</param>
    /// <param name="request">The request to add the parameter to</param>
    /// <param name="name">Name of the parameter; must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddUrlSegment<T>(this RestRequest request, IRestClient client, string name, T value, bool encode = true) where T : struct
        => request.AddUrlSegment(name, value.ToStringWithCulture(client.Options.CultureForParameters), encode);
}
