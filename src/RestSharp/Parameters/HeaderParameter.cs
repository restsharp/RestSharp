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

using System.Text;
using System.Text.RegularExpressions;

namespace RestSharp;

public partial record HeaderParameter : Parameter {
    /// <summary>
    /// Instantiates a header parameter
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <param name="encode">Set to true to encode header value according to RFC 2047. Default is false.</param>
    public HeaderParameter(string name, string value, bool encode = false)
        : base(
            EnsureValidHeaderString(Ensure.NotEmptyString(name, nameof(name)), "name"),
            EnsureValidHeaderValue(name, value, encode),
            ParameterType.HttpHeader,
            false
        ) { }

    public new string Name  => base.Name!;
    public new string Value => (string)base.Value!;

    static string EnsureValidHeaderValue(string name, string value, bool encode) {
        CheckAndThrowsForInvalidHost(name, value);

        return EnsureValidHeaderString(GetValue(Ensure.NotNull(value, nameof(value)), encode), "value");
    }

    static string EnsureValidHeaderString(string value, string type)
        => !IsInvalidHeaderString(value) ? value : throw new ArgumentException($"Invalid character found in header {type}: {value}");

    static string GetValue(string value, bool encode) => encode ? GetBase64EncodedHeaderValue(value) : value;

    static string GetBase64EncodedHeaderValue(string value) => $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";

    static bool IsInvalidHeaderString(string stringValue) {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < stringValue.Length; i++) {
            switch (stringValue[i]) {
                case '\r':
                case '\n':
                    return true;
            }
        }

        return false;
    }

    static readonly Regex PortSplitRegex = PartSplit();

    static void CheckAndThrowsForInvalidHost(string name, string value) {
        if (name == KnownHeaders.Host && InvalidHost(value))
            throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));

        return;

        static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@":\d+")]
    private static partial Regex PartSplit();
#else
    static Regex PartSplit() => new(@":\d+");
#endif
}