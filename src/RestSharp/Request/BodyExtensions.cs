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

static class BodyExtensions {
    public static bool TryGetBodyParameter(this RestRequest request, out BodyParameter? bodyParameter) {
        bodyParameter = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody) as BodyParameter;
        return bodyParameter != null;
    }

    public static bool HasFiles(this RestRequest request) => request.Files.Count > 0;

    public static bool IsEmpty(this ParametersCollection? parameters) => parameters == null || parameters.Count == 0;
}