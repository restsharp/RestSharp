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

static class ParametersCollectionExtensions {
    extension(ParametersCollection parameters) {
        internal IEnumerable<Parameter> GetQueryParameters(Method method) {
            Func<Parameter, bool> condition =
                !IsPost(method)
                    ? p => p.Type is ParameterType.GetOrPost or ParameterType.QueryString
                    : p => p.Type is ParameterType.QueryString;

            return parameters.Where(p => condition(p));
        }

        internal IEnumerable<GetOrPostParameter> GetContentParameters(Method method)
            => IsPost(method) ? parameters.GetParameters<GetOrPostParameter>() : [];
    }

    static bool IsPost(Method method) => method is Method.Post or Method.Put or Method.Patch;
}
