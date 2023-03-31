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

public sealed class RequestParameters : ParametersCollection {
    public RequestParameters() { }

    public RequestParameters(IEnumerable<Parameter> parameters) => Parameters.AddRange(parameters);

    public ParametersCollection AddParameters(IEnumerable<Parameter> parameters) {
        Parameters.AddRange(parameters);
        return this;
    }

    public ParametersCollection AddParameters(ParametersCollection parameters) {
        Parameters.AddRange(parameters);
        return this;
    }

    public void AddParameter(Parameter parameter) => Parameters.Add(parameter);

    public void RemoveParameter(string name) => Parameters.RemoveAll(x => x.Name == name);

    public void RemoveParameter(Parameter parameter) => Parameters.Remove(parameter);
}
