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

    /// <summary>
    /// Safely add a default parameter to the collection.
    /// </summary>
    /// <param name="parameter">Parameter to add</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public DefaultParameters AddParameter(Parameter parameter) {
        lock (_lock) {
            if (parameter.Type == ParameterType.RequestBody)
                throw new NotSupportedException(
                    "Cannot set request body using default parameters. Use Request.AddBody() instead."
                );

            if (!_options.AllowMultipleDefaultParametersWithSameName &&
                !MultiParameterTypes.Contains(parameter.Type)        &&
                this.Any(x => x.Name == parameter.Name)) {
                throw new ArgumentException("A default parameters with the same name has already been added", nameof(parameter));
            }

            Parameters.Add(parameter);
        }

        return this;
    }

    /// <summary>
    /// Safely removes all the default parameters with the given name and type.
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="type">Parameter type</param>
    /// <returns></returns>
    [PublicAPI]
    public DefaultParameters RemoveParameter(string name, ParameterType type) {
        lock (_lock) {
            Parameters.RemoveAll(x => x.Name == name && x.Type == type);
        }

        return this;
    }

    /// <summary>
    /// Replace a default parameter with the same name and type.
    /// </summary>
    /// <param name="parameter">Parameter instance</param>
    /// <returns></returns>
    [PublicAPI]
    public DefaultParameters ReplaceParameter(Parameter parameter)
        =>
            // ReSharper disable once NotResolvedInText
            RemoveParameter(Ensure.NotEmptyString(parameter.Name, "Parameter name"), parameter.Type)
                .AddParameter(parameter);

    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };
}
