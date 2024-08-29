// Copyright (c) .NET Foundation and Contributors
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

namespace RestSharp;

public static partial class RestRequestExtensions {
    /// <summary>
    /// Adds a header to the request. RestSharp will try to separate request and content headers when calling the resource.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="values">Header values</param>
    /// <returns></returns>
    public static RestRequest AddHeader(this RestRequest request, string name, string[] values) {
        foreach (var value in values) {
            AddHeader(request, name, value);
        }

        return request;
    }

    /// <summary>
    /// Adds a header to the request. RestSharp will try to separate request and content headers when calling the resource.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddHeader(this RestRequest request, string name, string value)
        => request.AddParameter(new HeaderParameter(name, value));

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
    /// The existing header with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateHeader(this RestRequest request, string name, string value)
        => request.AddOrUpdateParameter(new HeaderParameter(name, value));

    /// <summary>
    /// Adds or updates the request header. RestSharp will try to separate request and content headers when calling the resource.
    /// The existing header with the same name will be replaced.
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

    static void CheckAndThrowsDuplicateKeys(ICollection<KeyValuePair<string, string>> headers) {
        var duplicateKeys = headers
            .GroupBy(pair => pair.Key.ToUpperInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateKeys.Count > 0) {
            throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");
        }
    }
}