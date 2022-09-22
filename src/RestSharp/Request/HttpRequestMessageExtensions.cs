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

using System.Net.Http.Headers;
using RestSharp.Extensions;

namespace RestSharp;

static class HttpRequestMessageExtensions {
    public static void AddHeaders(this HttpRequestMessage message, RequestHeaders headers) {
        var headerParameters = headers.Parameters.Where(x => !KnownHeaders.IsContentHeader(x.Name!));

        headerParameters.ForEach(x => AddHeader(x, message.Headers));

        void AddHeader(Parameter parameter, HttpHeaders httpHeaders) {
            var parameterStringValue = parameter.Value!.ToString();

            httpHeaders.Remove(parameter.Name!);
            httpHeaders.TryAddWithoutValidation(parameter.Name!, parameterStringValue);
        }
    }
}
