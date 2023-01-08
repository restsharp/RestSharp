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
using System.Text.RegularExpressions;

namespace RestSharp;

[PublicAPI]
public static class RestRequestExtensions {
    static readonly Regex PortSplitRegex = new(@":\d+");

    /// <summary>
    /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddParameter(this RestRequest request, string name, string? value, bool encode = true)
        => request.AddParameter(new GetOrPostParameter(name, value, encode));

    /// <summary>
    /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddParameter<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddParameter(name, value.ToString(), encode);

    /// <summary>
    /// Adds or updates a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, string? value, bool encode = true)
        => request.AddOrUpdateParameter(new GetOrPostParameter(name, value, encode));

    /// <summary>
    /// Adds or updates a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddOrUpdateParameter<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddOrUpdateParameter(name, value.ToString(), encode);

    /// <summary>
    /// Adds a URL segment parameter to the request. The resource URL must have a placeholder for the parameter for it to work.
    /// For example, if you add a URL segment parameter with the name "id", the resource URL should contain {id} in its path.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddUrlSegment(this RestRequest request, string name, string value, bool encode = true)
        => request.AddParameter(new UrlSegmentParameter(name, value, encode));

    /// <summary>
    /// Adds a URL segment parameter to the request. The resource URL must have a placeholder for the parameter for it to work.
    /// For example, if you add a URL segment parameter with the name "id", the resource URL should contain {id} in its path.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddUrlSegment<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddUrlSegment(name, Ensure.NotNull(value.ToString(), nameof(value)), encode);

    /// <summary>
    /// Adds a query string parameter to the request. The request resource should not contain any placeholders for this parameter.
    /// The parameter will be added to the request URL as a query string using name=value format.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddQueryParameter(this RestRequest request, string name, string? value, bool encode = true)
        => request.AddParameter(new QueryParameter(name, value, encode));

    /// <summary>
    /// Adds a query string parameter to the request. The request resource should not contain any placeholders for this parameter.
    /// The parameter will be added to the request URL as a query string using name=value format.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddQueryParameter<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddQueryParameter(name, value.ToString(), encode);

    /// <summary>
    /// Adds a header to the request. RestSharp will try to separate request and content headers when calling the resource.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddHeader(this RestRequest request, string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddParameter(new HeaderParameter(name, value));
    }

    /// <summary>
    /// Adds a header to the request. RestSharp will try to separate request and content headers when calling the resource.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddHeader<T>(this RestRequest request, string name, T value) where T : struct
        => request.AddHeader(name, Ensure.NotNull(value.ToString(), nameof(value)));

    /// <summary>
    /// Adds or updates the request header. RestSharp will try to separate request and content headers when calling the resource.
    /// Existing header with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateHeader(this RestRequest request, string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddOrUpdateParameter(new HeaderParameter(name, value));
    }

    /// <summary>
    /// Adds or updates the request header. RestSharp will try to separate request and content headers when calling the resource.
    /// Existing header with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateHeader<T>(this RestRequest request, string name, T value) where T : struct
        => request.AddOrUpdateHeader(name, Ensure.NotNull(value.ToString(), nameof(value)));

    /// <summary>
    /// Adds multiple headers to the request, using the key-value pairs provided.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="headers">Collection of key-value pairs, where key will be used as header name, and value as header value</param>
    /// <returns></returns>
    public static RestRequest AddHeaders(this RestRequest request, ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var header in headers) {
            request.AddHeader(header.Key, header.Value);
        }

        return request;
    }

    /// <summary>
    /// Adds or updates multiple headers to the request, using the key-value pairs provided. Existing headers with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="headers">Collection of key-value pairs, where key will be used as header name, and value as header value</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateHeaders(this RestRequest request, ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var pair in headers) {
            request.AddOrUpdateHeader(pair.Key, pair.Value);
        }

        return request;
    }

    /// <summary>
    /// Adds a parameter of a given type to the request. It will create a typed parameter instance based on the type argument.
    /// It is not recommended to use this overload unless you must, as it doesn't provide any restrictions, and if the name-value-type
    /// combination doesn't match, it will throw.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">Enum value specifying what kind of parameter is being added</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddParameter(this RestRequest request, string? name, object value, ParameterType type, bool encode = true)
        => type == ParameterType.RequestBody
            ? request.AddBodyParameter(name, value)
            : request.AddParameter(Parameter.CreateParameter(name, value, type, encode));

    static RestRequest AddBodyParameter(this RestRequest request, string? name, object value)
        => name != null && name.Contains("/")
            ? request.AddBody(value, name)
            : request.AddParameter(new BodyParameter(name, value, ContentType.Plain));

    /// <summary>
    /// Adds or updates request parameter of a given type. It will create a typed parameter instance based on the type argument.
    /// Parameter will be added or updated based on its name. If the request has a parameter with the same name, it will be updated.
    /// It is not recommended to use this overload unless you must, as it doesn't provide any restrictions, and if the name-value-type
    /// combination doesn't match, it will throw.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">Enum value specifying what kind of parameter is being added</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value, ParameterType type, bool encode = true) {
        request.RemoveParameter(name, type);

        return type == ParameterType.RequestBody
            ? request.AddBodyParameter(name, value)
            : request.AddOrUpdateParameter(Parameter.CreateParameter(name, value, type, encode));
    }

    /// <summary>
    /// Adds or updates request parameter, given the parameter instance, for example <see cref="QueryParameter"/> or <see cref="UrlSegmentParameter"/>.
    /// It will replace an existing parameter with the same name.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="parameter">Parameter instance</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, Parameter parameter)
        => request.RemoveParameter(parameter.Name, parameter.Type).AddParameter(parameter);

    static RestRequest RemoveParameter(this RestRequest request, string? name, ParameterType type) {
        var p = request.Parameters.FirstOrDefault(x => x.Name == name && x.Type == type);
        return p != null ? request.RemoveParameter(p) : request;
    }

    /// <summary>
    /// Adds or updates multiple request parameters, given the parameter instance, for example
    /// <see cref="QueryParameter"/> or <see cref="UrlSegmentParameter"/>. Parameters with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="parameters">Collection of parameter instances</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameters(this RestRequest request, IEnumerable<Parameter> parameters) {
        foreach (var parameter in parameters) request.AddOrUpdateParameter(parameter);

        return request;
    }

    /// <summary>
    /// Adds a file parameter to the request body. The file will be read from disk as a stream.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="path">Full path to the file</param>
    /// <param name="contentType">Optional: content type</param>
    /// <param name="options">File parameter header options</param>
    /// <returns></returns>
    public static RestRequest AddFile(
        this RestRequest      request,
        string                name,
        string                path,
        ContentType?          contentType = null,
        FileParameterOptions? options     = null
    )
        => request.AddFile(FileParameter.FromFile(path, name, contentType, options));

    /// <summary>
    /// Adds bytes to the request as file attachment
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="bytes">File content as bytes</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
    /// <param name="options">File parameter header options</param>
    /// <returns></returns>
    public static RestRequest AddFile(
        this RestRequest      request,
        string                name,
        byte[]                bytes,
        string                filename,
        ContentType?          contentType = null,
        FileParameterOptions? options     = null
    )
        => request.AddFile(FileParameter.Create(name, bytes, fileName, contentType, options));

    /// <summary>
    /// Adds a file attachment to the request, where the file content will be retrieved from a given stream 
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="getFile">Function that returns a stream with the file content</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
    /// <param name="options">File parameter header options</param>
    /// <returns></returns>
    public static RestRequest AddFile(
        this RestRequest      request,
        string                name,
        Func<Stream>          getFile,
        string                fileName,
        ContentType?          contentType = null,
        FileParameterOptions? options     = null
    )
        => request.AddFile(FileParameter.Create(name, getFile, fileName, contentType, options));

