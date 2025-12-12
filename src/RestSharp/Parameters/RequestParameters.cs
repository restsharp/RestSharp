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

/// <summary>
/// Collection of request parameters
/// </summary>
public sealed class RequestParameters : ParametersCollection {
    /// <summary>
    /// Create an empty parameters collection
    /// </summary>
    public RequestParameters() { }

    /// <summary>
    /// Creates a parameters collection from a collection of parameter objects
    /// </summary>
    /// <param name="parameters">Collection of existing parameters</param>
    public RequestParameters(IEnumerable<Parameter> parameters) => Parameters.AddRange(parameters);

    /// <summary>
    /// Adds multiple parameters to the collection
    /// </summary>
    /// <param name="parameters">Parameters to add</param>
    /// <returns></returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public ParametersCollection AddParameters(IEnumerable<Parameter> parameters) {
        Parameters.AddRange(parameters);
        return this;
    }

    /// <summary>
    /// Add parameters from another parameter collection
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public ParametersCollection AddParameters(ParametersCollection parameters) {
        Parameters.AddRange(parameters);
        return this;
    }

    /// <summary>
    /// Adds a single parameter to the collection
    /// </summary>
    /// <param name="parameter">Parameter to add</param>
    public void AddParameter(Parameter parameter) => Parameters.Add(parameter);

    /// <summary>
    /// Remove one or more parameters from the collection by name
    /// </summary>
    /// <param name="name">Name of the parameter to remove</param>
    // ReSharper disable once UnusedMember.Global
    public void RemoveParameter(string name) => Parameters.RemoveAll(x => x.Name == name);

    /// <summary>
    /// Remove parameter from the collection by reference
    /// </summary>
    /// <param name="parameter">Parameter to remove</param>
    public void RemoveParameter(Parameter parameter) => Parameters.Remove(parameter);
}
