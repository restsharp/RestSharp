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

public record GetOrPostParameter : NamedParameter {
    /// <summary>
    /// Instantiates an HTTP parameter instance (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    public GetOrPostParameter(string name, string? value, bool encode = true) : base(name, value, ParameterType.GetOrPost, encode) { }
}