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
using RestSharp.Extensions;
using RestSharp.Serializers;

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

    // TODO: Three methods below added for binary compatibility with v108. Remove for the next major release.
    // In addition, both contentType and options parameters should get default values.

    public static RestRequest AddFile(
        this RestRequest request,
        string           name,
        string           path,
        string?          contentType = null
    )
        => request.AddFile(FileParameter.FromFile(path, name, contentType));

    public static RestRequest AddFile(
        this RestRequest request,
        string           name,
        byte[]           bytes,
        string           filename,
        string?          contentType = null
    )
        => request.AddFile(FileParameter.Create(name, bytes, filename, contentType));

    public static RestRequest AddFile(
        this RestRequest      request,
        string                name,
        Func<Stream>          getFile,
        string                fileName,
        string?               contentType = null
    )
        => request.AddFile(FileParameter.Create(name, getFile, fileName, contentType));

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
        string?               contentType,
        FileParameterOptions? options
    )
        => request.AddFile(FileParameter.FromFile(path, name, contentType, options));

    /// <summary>
    /// Adds bytes to the request as file attachment
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="bytes">File content as bytes</param>
    /// <param name="filename">File name</param>
    /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
    /// <param name="options">File parameter header options</param>
    /// <returns></returns>
    public static RestRequest AddFile(
        this RestRequest      request,
        string                name,
        byte[]                bytes,
        string                filename,
        string?               contentType,
        FileParameterOptions? options
    )
        => request.AddFile(FileParameter.Create(name, bytes, filename, contentType, options));

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
        string?               contentType,
        FileParameterOptions? options
    )
        => request.AddFile(FileParameter.Create(name, getFile, fileName, contentType, options));

    /// <summary>
    /// Adds a body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object to be used as the request body, or string for plain content</param>
    /// <param name="contentType">Optional: content type</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if request body type cannot be resolved</exception>
    /// <remarks>This method will try to figure out the right content type based on the request data format and the provided content type</remarks>
    public static RestRequest AddBody(this RestRequest request, object obj, string? contentType = null) {
        if (contentType == null) {
            return request.RequestFormat switch {
                DataFormat.Json   => request.AddJsonBody(obj),
                DataFormat.Xml    => request.AddXmlBody(obj),
                DataFormat.Binary => request.AddParameter(new BodyParameter("", obj, ContentType.Binary)),
                _                 => request.AddParameter(new BodyParameter("", obj.ToString()!, ContentType.Plain))
            };
        }

        return
            obj is string str            ? request.AddStringBody(str, contentType) :
            obj is byte[] bytes          ? request.AddParameter(new BodyParameter("", bytes, contentType, DataFormat.Binary)) :
            contentType.Contains("xml")  ? request.AddXmlBody(obj, contentType) :
            contentType.Contains("json") ? request.AddJsonBody(obj, contentType) :
                                           throw new ArgumentException("Non-string body found with unsupported content type", nameof(obj));
    }

    /// <summary>
    /// Adds a string body and figures out the content type from the data format specified. You can, for example, add a JSON string
    /// using this method as request body, using DataFormat.Json/>
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="body">String body</param>
    /// <param name="dataFormat"><see cref="DataFormat"/> for the content</param>
    /// <returns></returns>
    public static RestRequest AddStringBody(this RestRequest request, string body, DataFormat dataFormat) {
        var contentType = ContentType.FromDataFormat[dataFormat];
        request.RequestFormat = dataFormat;
        return request.AddParameter(new BodyParameter("", body, contentType));
    }

    /// <summary>
    /// Adds a string body to the request using the specified content type.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="body">String body</param>
    /// <param name="contentType">Content type of the body</param>
    /// <returns></returns>
    public static RestRequest AddStringBody(this RestRequest request, string body, string contentType)
        => request.AddParameter(new BodyParameter(body, Ensure.NotEmpty(contentType, nameof(contentType))));

    /// <summary>
    /// Adds a JSON body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to JSON</param>
    /// <param name="contentType">Optional: content type. Default is "application/json"</param>
    /// <returns></returns>
    public static RestRequest AddJsonBody<T>(this RestRequest request, T obj, string contentType = ContentType.Json) where T : class {
        request.RequestFormat = DataFormat.Json;
        return obj is string str ? request.AddStringBody(str, DataFormat.Json) : request.AddParameter(new JsonParameter(obj, contentType));
    }

    /// <summary>
    /// Adds an XML body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to XML</param>
    /// <param name="contentType">Optional: content type. Default is "application/xml"</param>
    /// <param name="xmlNamespace">Optional: XML namespace</param>
    /// <returns></returns>
    public static RestRequest AddXmlBody<T>(this RestRequest request, T obj, string contentType = ContentType.Xml, string xmlNamespace = "")
        where T : class {
        request.RequestFormat = DataFormat.Xml;

        return obj is string str
            ? request.AddStringBody(str, DataFormat.Xml)
            : request.AddParameter(new XmlParameter(obj, xmlNamespace, contentType));
    }

    /// <summary>
    /// Gets object properties and adds each property as a form data parameter
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object to add as form data</param>
    /// <param name="includedProperties">Properties to include, or nothing to include everything</param>
    /// <returns></returns>
    public static RestRequest AddObject<T>(this RestRequest request, T obj, params string[] includedProperties) where T : class {
        var props = obj.GetProperties(includedProperties);

        foreach (var (name, value) in props) {
            request.AddParameter(name, value);
        }

        return request;
    }

    /// <summary>
    /// Adds cookie to the <seealso cref="HttpClient"/> cookie container.
    /// </summary>
    /// <param name="request">RestRequest to add the cookies to</param>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    /// <param name="path">Cookie path</param>
    /// <param name="domain">Cookie domain, must not be an empty string</param>
    /// <returns></returns>
    public static RestRequest AddCookie(this RestRequest request, string name, string value, string path, string domain) {
        request.CookieContainer ??= new CookieContainer();
        request.CookieContainer.Add(new Cookie(name, value, path, domain));
        return request;
    }

    static void CheckAndThrowsForInvalidHost(string name, string value) {
        static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;

        if (name == KnownHeaders.Host && InvalidHost(value))
            throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));
    }

    static void CheckAndThrowsDuplicateKeys(ICollection<KeyValuePair<string, string>> headers) {
        var duplicateKeys = headers
            .GroupBy(pair => pair.Key.ToUpperInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateKeys.Any()) throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");
    }
}
