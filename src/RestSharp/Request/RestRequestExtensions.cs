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
    public static RestRequest AddParameter(this RestRequest request, string name, object? value, bool encode = true)
        => request.AddParameter(new GetOrPostParameter(name, value?.ToString(), encode));

    public static RestRequest AddParameter(this RestRequest request, string? name, object value, ParameterType type, bool encode = true)
        => request.AddParameter(Parameter.CreateParameter(name, value, type, encode));

    public static RestRequest AddOrUpdateParameter(this RestRequest request, Parameter parameter) {
        var p = request.Parameters.FirstOrDefault(x => x.Name == parameter.Name && x.Type == parameter.Type);

        if (p != null) request.RemoveParameter(p);

        request.AddParameter(parameter);
        return request;
    }

    public static RestRequest AddOrUpdateParameters(this RestRequest request, IEnumerable<Parameter> parameters) {
        foreach (var parameter in parameters)
            request.AddOrUpdateParameter(parameter);

        return request;
    }

    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object? value)
        => request.AddOrUpdateParameter(new GetOrPostParameter(name, value?.ToString()));

    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value, ParameterType type, bool encode = true)
        => request.AddOrUpdateParameter(Parameter.CreateParameter(name, value, type, encode));

    public static RestRequest AddHeader(this RestRequest request, string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddParameter(new HeaderParameter(name, value));
    }

    public static RestRequest AddOrUpdateHeader(this RestRequest request, string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddOrUpdateParameter(new HeaderParameter(name, value));
    }

    public static RestRequest AddHeaders(this RestRequest request, ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var pair in headers) {
            request.AddHeader(pair.Key, pair.Value);
        }

        return request;
    }

    public static RestRequest AddOrUpdateHeaders(this RestRequest request, ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var pair in headers) {
            request.AddOrUpdateHeader(pair.Key, pair.Value);
        }

        return request;
    }

    public static RestRequest AddUrlSegment(this RestRequest request, string name, string value, bool encode = true)
        => request.AddParameter(new UrlSegmentParameter(name, value, encode));

    public static RestRequest AddQueryParameter(this RestRequest request, string name, string value, bool encode = true)
        => request.AddParameter(new QueryParameter(name, value, encode));

    /// <summary>
    /// Adds a file parameter to the request body. The file will be read from disk as a stream.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="path">Full path to the file</param>
    /// <param name="contentType">Optional: content type</param>
    /// <returns></returns>
    public static RestRequest AddFile(this RestRequest request, string name, string path, string? contentType = null)
        => request.AddFile(FileParameter.FromFile(path, name, contentType));

    /// <summary>
    /// Adds bytes to the request as file attachment
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="bytes">File content as bytes</param>
    /// <param name="filename">File name</param>
    /// <param name="contentType">Optional: content type. Default is "application/octet-stream"</param>
    /// <returns></returns>
    public static RestRequest AddFile(this RestRequest request, string name, byte[] bytes, string filename, string? contentType = null)
        => request.AddFile(FileParameter.Create(name, bytes, filename, contentType));

    public static RestRequest AddFile(
        this RestRequest request,
        string           name,
        Func<Stream>     getFile,
        string           fileName,
        long             contentLength,
        string?          contentType = null
    )
        => request.AddFile(FileParameter.Create(name, getFile, contentLength, fileName, contentType));

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
                DataFormat.Json => request.AddJsonBody(obj, contentType ?? ContentType.Json),
                DataFormat.Xml  => request.AddXmlBody(obj, contentType ?? ContentType.Xml),
                _               => request.AddParameter(new BodyParameter("", obj.ToString()!, contentType ?? ContentType.Plain))
            };
        }

        return
            obj is string str            ? request.AddParameter(new BodyParameter("", str, contentType)) :
            contentType.Contains("xml")  ? request.AddXmlBody(obj, contentType) :
            contentType.Contains("json") ? request.AddJsonBody(obj, contentType) :
                                           throw new ArgumentException("Non-string body found with unsupported content type", nameof(obj));
    }

    /// <summary>
    /// Adds a JSON body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to JSON</param>
    /// <param name="contentType">Optional: content type. Default is "application/json"</param>
    /// <returns></returns>
    public static RestRequest AddJsonBody(this RestRequest request, object obj, string contentType = ContentType.Json) {
        request.RequestFormat = DataFormat.Json;
        return request.AddParameter(new JsonParameter("", obj, contentType));
    }

    /// <summary>
    /// Adds an XML body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to XML</param>
    /// <param name="contentType">Optional: content type. Default is "application/xml"</param>
    /// <param name="xmlNamespace">Optional: XML namespace</param>
    /// <returns></returns>
    public static RestRequest AddXmlBody(this RestRequest request, object obj, string contentType = ContentType.Xml, string xmlNamespace = "") {
        request.RequestFormat = DataFormat.Xml;
        request.AddParameter(new XmlParameter("", obj, xmlNamespace, contentType));
        return request;
    }

    /// <summary>
    /// Gets object properties and adds each property as a form data parameter
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object to add as form data</param>
    /// <param name="includedProperties">Properties to include, or nothing to include everything</param>
    /// <returns></returns>
    public static RestRequest AddObject(this RestRequest request, object obj, params string[] includedProperties) {
        var props = obj.GetProperties(includedProperties);

        foreach (var (name, value) in props) {
            request.AddParameter(name, value);
        }

        return request;
    }

    static void CheckAndThrowsForInvalidHost(string name, string value) {
        static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;

        if (name == "Host" && InvalidHost(value))
            throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));
    }

    static void CheckAndThrowsDuplicateKeys(ICollection<KeyValuePair<string, string>> headers) {
        var duplicateKeys = headers
            .GroupBy(pair => pair.Key.ToUpperInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateKeys.Any())
            throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");
    }
}