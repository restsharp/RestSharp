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

using System.Collections;

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

    static readonly Func<Parameter, string?, bool> SearchPredicate = (p, name)
        => p.Name != null && p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);

    public bool Exists(Parameter parameter) => _parameters.Any(p => SearchPredicate(p, parameter.Name) && p.Type == parameter.Type);

    public Parameter? TryFind(string parameterName) => _parameters.FirstOrDefault(x => SearchPredicate(x, parameterName));

    public ParametersCollection GetParameters(ParameterType parameterType) => new(_parameters.Where(x => x.Type == parameterType));

    public ParametersCollection GetParameters<T>() => new(_parameters.Where(x => x is T));

    internal ParametersCollection GetQueryParameters(Method method) {
        Func<Parameter, bool> condition =
            !IsPost(method)
                ? p => p.Type is ParameterType.GetOrPost or ParameterType.QueryString
                : p => p.Type is ParameterType.QueryString;

        return new ParametersCollection(_parameters.Where(p => condition(p)));
    }

    internal ParametersCollection? GetContentParameters(Method method)
        => IsPost(method) ? new ParametersCollection(GetParameters<GetOrPostParameter>()) : null;

    static bool IsPost(Method method) => method is Method.Post or Method.Put or Method.Patch;

    public IEnumerator<Parameter> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _parameters.Count;
}