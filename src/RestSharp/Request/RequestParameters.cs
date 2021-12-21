//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

class RequestParameters {
    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };

    public ParametersCollection Parameters { get; } = new();

    public RequestParameters AddParameters(ParametersCollection parameters, bool allowSameName) {
        Parameters.AddParameters(GetParameters(parameters, allowSameName));
        return this;
    }

    IEnumerable<Parameter> GetParameters(ParametersCollection parametersCollection, bool allowSameName) {
        foreach (var parameter in parametersCollection) {
            var parameterExists = Parameters.Exists(parameter);

            if (allowSameName) {
                var isMultiParameter = MultiParameterTypes.Any(pt => pt == parameter.Type);
                parameterExists = !isMultiParameter && parameterExists;
            }

            if (!parameterExists) yield return parameter;
        }
    }

    // Add Accept header based on registered deserializers if none has been set by the caller.
    public RequestParameters AddAcceptHeader(string[] acceptedContentTypes) {
        if (Parameters.TryFind(KnownHeaders.Accept) == null) {
            var accepts = string.Join(", ", acceptedContentTypes);
            Parameters.AddParameter(new Parameter(KnownHeaders.Accept, accepts, ParameterType.HttpHeader));
        }

        return this;
    }
}