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

namespace RestSharp;

public record HeaderParameter : Parameter {
    /// <summary>
    /// Instantiates a header parameter
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    public HeaderParameter(string name, string value)
        : base(
            Ensure.NotEmptyString(name, nameof(name)),
            Ensure.NotNull(value, nameof(value)),
            ParameterType.HttpHeader,
            false
        ) { }

    public new string Name  => base.Name!;
    public new string Value => (string)base.Value!;
}