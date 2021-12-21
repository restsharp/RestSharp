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
// 

using System.Collections;
using System.Collections.Concurrent;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp;

public class ParametersCollection : IReadOnlyCollection<Parameter> {
    readonly List<Parameter> _parameters = new();

    public ParametersCollection() { }

    public ParametersCollection(IEnumerable<Parameter> parameters) => _parameters.AddRange(parameters);

    public ParametersCollection AddParameters(IEnumerable<Parameter> parameters) {
        _parameters.AddRange(parameters);
        return this;
    }

    public ParametersCollection AddParameters(ParametersCollection parameters) {
        _parameters.AddRange(parameters);
        return this;
    }

    public void AddParameter(Parameter parameter) => _parameters.Add(parameter);

    public void RemoveParameter(string name) => _parameters.RemoveAll(x => x.Name == name);

    public void RemoveParameter(Parameter parameter) => _parameters.Remove(parameter);

    public bool Exists(Parameter parameter)
        => _parameters.Any(
            p => p.Name != null && p.Name.Equals(parameter.Name, StringComparison.InvariantCultureIgnoreCase) && p.Type == parameter.Type
        );

    public Parameter? TryFind(string parameterName) => _parameters.FirstOrDefault(x => x.Name != null && x.Name.EqualsIgnoreCase(parameterName));

    internal ParametersCollection GetParameters(ParameterType parameterType) => new(_parameters.Where(x => x.Type == parameterType));

    internal ParametersCollection GetQueryParameters(Method method)
        => new(
            method is not Method.Post and not Method.Put and not Method.Patch
                ? _parameters
                    .Where(
                        p => p.Type is ParameterType.GetOrPost or ParameterType.QueryString
                    )
                : _parameters
                    .Where(
                        p => p.Type is ParameterType.QueryString
                    )
        );

    public IEnumerator<Parameter> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _parameters.Count;
}