// Copyright (c) .NET Foundation and Contributors
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

namespace RestSharp.Interceptors;

/// <summary>
/// This class allows easier migration of legacy request hooks to interceptors.
/// </summary>
public class CompatibilityInterceptor : Interceptor {
    public Action<RestResponse>? OnBeforeDeserialization { get; set; }
    public Func<HttpRequestMessage, ValueTask>? OnBeforeRequest { get; set; }
    public Func<HttpResponseMessage, ValueTask>? OnAfterRequest { get; set; }

    /// <inheritdoc />
    public override ValueTask BeforeDeserialization(RestResponse response, CancellationToken cancellationToken) {
        OnBeforeDeserialization?.Invoke(response);
        return default;
    }

    public override ValueTask BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken) {
        OnBeforeRequest?.Invoke(requestMessage);
        return default;
    }
    
    public override ValueTask AfterHttpRequest(HttpResponseMessage responseMessage, CancellationToken cancellationToken) {
        OnAfterRequest?.Invoke(responseMessage);
        return default;
    }
}