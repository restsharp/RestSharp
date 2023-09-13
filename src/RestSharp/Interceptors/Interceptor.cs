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

namespace RestSharp.Interceptors; 

/// <summary>
/// Base Interceptor
/// </summary>
public abstract class Interceptor {
    /// <summary>
    /// Intercepts the request before serialization
    /// </summary>
    /// <param name="request">RestRequest before serialization</param>
    /// <returns>Value Tags</returns>
    public virtual ValueTask InterceptBeforeSerialization(RestRequest request) {
        return new();
    }

    /// <summary>
    /// Intercepts the request before being sent
    /// </summary>
    /// <param name="req">HttpRequestMessage before being sent</param>
    /// <returns>Value Tags</returns>
    public virtual ValueTask InterceptBeforeRequest(HttpRequestMessage req) {
        return new();
    }

    /// <summary>
    /// Intercepts the request before being sent
    /// </summary>
    /// <param name="responseMessage">HttpResponseMessage as received from Server</param>
    /// <returns>Value Tags</returns>
    public virtual ValueTask InterceptAfterRequest(HttpResponseMessage responseMessage) {
        return new();
    }

    /// <summary>
    /// Intercepts the request before deserialization
    /// </summary>
    /// <param name="response">HttpResponseMessage as received from Server</param>
    /// <returns>Value Tags</returns>
    public virtual ValueTask InterceptBeforeDeserialize(RestResponse response) {
        return new();
    }
}