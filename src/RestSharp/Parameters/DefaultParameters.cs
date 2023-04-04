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

public sealed class DefaultParameters : ParametersCollection {
    readonly ReadOnlyRestClientOptions _options;

    readonly object _lock = new();

    public DefaultParameters(ReadOnlyRestClientOptions options) => _options = options;

    public void AddParameter(Parameter parameter) {
        lock (_lock) {
            if (parameter.Type == ParameterType.RequestBody)
                throw new NotSupportedException(
                    "Cannot set request body using default parameters. Use Request.AddBody() instead."
                );

            if (!_options.AllowMultipleDefaultParametersWithSameName &&
                !MultiParameterTypes.Contains(parameter.Type)       &&
                this.Any(x => x.Name == parameter.Name)) {
                throw new ArgumentException("A default parameters with the same name has already been added", nameof(parameter));
            }

            Parameters.Add(parameter);
        }
    }

    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };
}
