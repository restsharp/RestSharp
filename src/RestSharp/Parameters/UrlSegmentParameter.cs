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

using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp;

public partial record UrlSegmentParameter : NamedParameter {
    static readonly Regex RegexPattern = Pattern();

    /// <summary>
    /// Instantiates a new query parameter instance that will be added to the request URL by replacing part of the absolute path.
    /// The request resource should have a placeholder {name} that will be replaced with the parameter value when the request is made.
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="encode">Optional: encode the value, default is true</param>
    /// <param name="replaceEncodedSlash">Optional: whether to replace all %2f and %2F in the parameter value with '/', default is true</param>
    public UrlSegmentParameter(string name, string? value, bool encode = true, bool replaceEncodedSlash = true)
        : base(
            name,
            value.IsEmpty() ? string.Empty : replaceEncodedSlash ? RegexPattern.Replace(value, "/") : value,
            ParameterType.UrlSegment,
            encode
        ) { }

#if NET7_0_OR_GREATER
    [GeneratedRegex("%2f", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-NO")]
    private static partial Regex Pattern();
#else
    static Regex Pattern() => new("%2f", RegexOptions.IgnoreCase | RegexOptions.Compiled);
#endif
}