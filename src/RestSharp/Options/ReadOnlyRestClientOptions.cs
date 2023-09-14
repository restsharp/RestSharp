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

using RestSharp.Interceptors;

namespace RestSharp;

public partial class ReadOnlyRestClientOptions {
    public IReadOnlyCollection<Interceptor>? Interceptors { get; private set; }

    // partial void CopyAdditionalProperties(RestClientOptions inner); 
    partial void CopyAdditionalProperties(RestClientOptions inner) => Interceptors = GetInterceptors(inner);

    static IReadOnlyCollection<Interceptor>? GetInterceptors(RestClientOptions? options) {
        if (options == null || options.Interceptors.Count == 0) return null;

        var interceptors = new List<Interceptor>(options.Interceptors);
        return interceptors.AsReadOnly();
    }
}
