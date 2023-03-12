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
using System.Net;

namespace RestSharp;

/// <summary>
/// Types of parameters that can be added to requests
/// </summary>
public enum ParameterType {
    /// <summary>
    /// A <see cref="Parameter"/> that will added to the QueryString for GET, DELETE, OPTIONS and HEAD requests; and form for POST and PUT requests.
    /// </summary>
    /// <remarks>
    /// See <see cref="GetOrPostParameter"/>.
    /// </remarks>
    GetOrPost,

    /// <summary>
    /// A <see cref="Parameter"/> that will be added to part of the url by replacing a <c>{placeholder}</c> within the absolute path.
    /// </summary>
    /// <remarks>
    /// See <see cref="UrlSegmentParameter"/>.
    /// </remarks>
    UrlSegment,

    /// <summary>
    /// A <see cref="Parameter"/> that will be added as a request header
    /// </summary>
    /// <remarks>
    /// See <see cref="HeaderParameter"/>.
    /// </remarks>
    HttpHeader,

    /// <summary>
    /// A <see cref="Parameter"/> that will be added to the request body
    /// </summary>
    /// <remarks>
    /// See <see cref="BodyParameter"/>.
    /// </remarks>
    RequestBody,

    /// <summary>
    /// A <see cref="Parameter"/> that will be added to the query string
    /// </summary>
    /// <remarks>
    /// See <see cref="QueryParameter"/>.
    /// </remarks>
    QueryString
}

/// <summary>
/// Data formats
/// </summary>
public enum DataFormat { Json, Xml, Binary, None }

/// <summary>
/// HTTP method to use when making requests
/// </summary>
public enum Method {
    Get, Post, Put, Delete, Head, Options,
    Patch, Merge, Copy, Search
}

/// <summary>
/// Format strings for commonly-used date formats
/// </summary>
public struct DateFormat {
    /// <summary>
    /// .NET format string for ISO 8601 date format
    /// </summary>
    public const string ISO_8601 = "s";

    /// <summary>
    /// .NET format string for roundtrip date format
    /// </summary>
    public const string ROUND_TRIP = "u";
}

/// <summary>
/// Status for responses (surprised?)
/// </summary>
public enum ResponseStatus { 
    /// <summary>
    /// Not Applicable, for when the Request has not yet been made
    /// </summary>
    None,

    /// <summary>
    /// <see cref="ResponseStatus"/> for when the request is passes as a result of <see cref="HttpResponseMessage.IsSuccessStatusCode"/> being true, or when the response is <see cref="HttpStatusCode.NotFound"/>
    /// </summary>
    Completed,

    /// <summary>
    /// <see cref="ResponseStatus"/> for when the request fails due as a result of <see cref="HttpResponseMessage.IsSuccessStatusCode"/> being false except for the case when the response is <see cref="HttpStatusCode.NotFound"/>
    /// </summary>
    Error, 
    
    /// <summary>
    /// <see cref="ResponseStatus"/> for when the Operation is cancelled due to the request taking longer than the length of time prescribed by <see cref="RestRequest.Timeout"/> or due to the <see cref="HttpClient"/> timing out.
    /// </summary>
    TimedOut,

    /// <summary>
    /// <see cref="ResponseStatus"/> for when the Operation is cancelled, due to reasons other than <see cref="TimedOut"/>
    /// </summary>
    Aborted
}