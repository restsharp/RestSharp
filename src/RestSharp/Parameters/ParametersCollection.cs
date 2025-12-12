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

public abstract class ParametersCollection<T> : IReadOnlyCollection<T> where T : Parameter {
    protected readonly List<T> Parameters = [];

    // public ParametersCollection(IEnumerable<Parameter> parameters) => _parameters.AddRange(parameters);

    static readonly Func<T, string?, bool> SearchPredicate = (p, name)
        => p.Name != null && p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);

    public bool Exists(T parameter) => Parameters.Any(p => SearchPredicate(p, parameter.Name) && p.Type == parameter.Type);

    public T? TryFind(string parameterName) => Parameters.FirstOrDefault(x => SearchPredicate(x, parameterName));

    public IEnumerable<TParameter> GetParameters<TParameter>() where TParameter : class, T => Parameters.OfType<TParameter>();

    public IEnumerator<T> GetEnumerator() => Parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Parameters.Count;
}

public abstract class ParametersCollection : ParametersCollection<Parameter> {
    public IEnumerable<Parameter> GetParameters(ParameterType parameterType) => Parameters.Where(x => x.Type == parameterType);
}