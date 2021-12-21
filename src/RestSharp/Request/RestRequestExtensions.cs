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
    /// <returns>This request</returns>
    public static RestRequest AddParameter(this RestRequest request, string name, object value, bool encode = true)
        => request.AddParameter(new Parameter(name, value, ParameterType.GetOrPost, encode));

    public static RestRequest AddParameter(this RestRequest request, string? name, object value, ParameterType type, bool encode = true)
        => request.AddParameter(new Parameter(name, value, type, encode));

    public static RestRequest AddParameter(this RestRequest request, string name, object value, string contentType, ParameterType type, bool encode = true)
        => request.AddParameter(new Parameter(name, value, contentType, type, encode));

    public static RestRequest AddOrUpdateParameter(this RestRequest request, Parameter parameter) {
        var p = request.Parameters .FirstOrDefault(x => x.Name == parameter.Name && x.Type == parameter.Type);

        if (p != null) request.RemoveParameter(p);

        request.AddParameter(parameter);
        return request;
    }

    public static RestRequest AddOrUpdateParameters(this RestRequest request, IEnumerable<Parameter> parameters) {
        foreach (var parameter in parameters)
            request.AddOrUpdateParameter(parameter);

        return request;
    }

    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value)
        => request.AddOrUpdateParameter(new Parameter(name, value, ParameterType.GetOrPost));

    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value, ParameterType type, bool encode = true)
        => request.AddOrUpdateParameter(new Parameter(name, value, type));

    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value, string contentType, ParameterType type, bool encode = true)
        => request.AddOrUpdateParameter(new Parameter(name, value, contentType, type, encode));

    public static RestRequest AddHeader(this RestRequest request, string name, string value, bool encode = true) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddParameter(name, value, ParameterType.HttpHeader, encode);
    }

    public static RestRequest AddOrUpdateHeader(this RestRequest request, string name, string value, bool encode = true) {
        CheckAndThrowsForInvalidHost(name, value);
        return request.AddOrUpdateParameter(name, value, ParameterType.HttpHeader, encode);
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

    public static RestRequest AddUrlSegment(this RestRequest request, string name, string value)
        => request.AddParameter(name, value, ParameterType.UrlSegment);

    public static RestRequest AddUrlSegment(this RestRequest request, string name, string value, bool encode) {
        var parameter = new Parameter(name, value, ParameterType.UrlSegment, encode);
        return request.AddParameter(parameter);
    }

    public static RestRequest AddQueryParameter(this RestRequest request, string name, string value)
        => request.AddParameter(name, value, ParameterType.QueryString);

    public static RestRequest AddQueryParameter(this RestRequest request, string name, string value, bool encode) {
        var parameter = new Parameter(name, value, ParameterType.QueryString, encode);
        return request.AddParameter(parameter);
    }

    public static RestRequest AddUrlSegment(this RestRequest request, string name, object value)
        => request.AddParameter(name, value, ParameterType.UrlSegment);

    public static RestRequest AddFile(this RestRequest request, string name, string path, string? contentType = null)
        => request.AddFile(FileParameter.FromFile(path, name, contentType));

    public static RestRequest AddFile(this RestRequest request, string name, byte[] bytes, string fileName, string? contentType = null)
        => request.AddFile(FileParameter.Create(name, bytes, fileName, contentType));

    public static RestRequest AddFile(
        this RestRequest request,
        string           name,
        Func<Stream>     getFile,
        string           fileName,
        long             contentLength,
        string?          contentType = null
    )
        => request.AddFile(FileParameter.Create(name, getFile, contentLength, fileName, contentType));

    public static RestRequest AddFileBytes(
        this RestRequest request,
        string           name,
        byte[]           bytes,
        string           filename,
        string           contentType = "application/x-gzip"
    )
        => request.AddFile(FileParameter.Create(name, bytes, filename, contentType));

    public static RestRequest AddBody(this RestRequest request, object obj, string xmlNamespace)
        => request.RequestFormat switch {
            DataFormat.Json => request.AddJsonBody(obj),
            DataFormat.Xml  => request.AddXmlBody(obj, xmlNamespace),
            _               => request
        };

    public static RestRequest AddBody(this RestRequest request, object obj)
        => request.RequestFormat switch {
            DataFormat.Json => request.AddJsonBody(obj),
            DataFormat.Xml  => request.AddXmlBody(obj),
            _               => request.AddParameter("", obj.ToString()!)
        };

    public static RestRequest AddJsonBody(this RestRequest request, object obj) {
        request.RequestFormat = DataFormat.Json;
        return request.AddParameter(new JsonParameter("", obj));
    }

    public static RestRequest AddJsonBody(this RestRequest request, object obj, string contentType) {
        request.RequestFormat = DataFormat.Json;
        return request.AddParameter(new JsonParameter(contentType, obj, contentType));
    }

    public static RestRequest AddXmlBody(this RestRequest request, object obj) => request.AddXmlBody(obj, "");

    public static RestRequest AddXmlBody(this RestRequest request, object obj, string xmlNamespace) {
        request.RequestFormat = DataFormat.Xml;
        request.AddParameter(new XmlParameter("", obj, xmlNamespace));
        return request;
    }

    public static RestRequest AddObject(this RestRequest request, object obj, params string[] includedProperties) {
        // automatically create parameters from object props
        var type  = obj.GetType();
        var props = type.GetProperties();

        foreach (var prop in props) {
            if (!IsAllowedProperty(prop.Name))
                continue;

            var val = prop.GetValue(obj, null);

            if (val == null)
                continue;

            var propType = prop.PropertyType;

            if (propType.IsArray) {
                var elementType = propType.GetElementType();
                var array       = (Array)val;

                if (array.Length > 0 && elementType != null) {
                    // convert the array to an array of strings
                    var values = array.Cast<object>().Select(item => item.ToString());

                    val = string.Join(",", values);
                }
            }

            request.AddParameter(prop.Name, val);
        }

        return request;

        bool IsAllowedProperty(string propertyName)
            => includedProperties.Length == 0 || includedProperties.Length > 0 && includedProperties.Contains(propertyName);
    }

    public static RestRequest AddObject(this RestRequest request, object obj) {
        return request.With(x => x.AddObject(obj, new string[] { }));
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