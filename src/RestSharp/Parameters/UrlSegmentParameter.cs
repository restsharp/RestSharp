//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

public record UrlSegmentParameter : NamedParameter {
    /// <summary>
    /// Instantiates a new query parameter instance that will be added to the request URL part of the query string.
    /// The request resource should have a placeholder {name} that will be replaced with the parameter value when the request is made.
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="encode">Optional: encode the value, default is true</param>
    public UrlSegmentParameter(string name, string value, bool encode = true)
        : base(name, Ensure.NotEmpty(value, nameof(value)).Replace("%2F", "/").Replace("%2f", "/"), ParameterType.UrlSegment, encode) { }
}