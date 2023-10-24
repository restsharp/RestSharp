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
    static readonly ValueTask Completed = 
#if NET
        ValueTask.CompletedTask;
#else
        new ();
#endif
    /// <summary>
    /// Intercepts the request before composing the request message
    /// </summary>
    /// <param name="request">RestRequest before composing the request message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual ValueTask BeforeRequest(RestRequest request, CancellationToken cancellationToken) => Completed;

    /// <summary>
    /// Intercepts the request before being sent
    /// </summary>
    /// <param name="requestMessage">HttpRequestMessage before being sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual ValueTask BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken) => Completed;

    /// <summary>
    /// Intercepts the request before being sent
    /// </summary>
    /// <param name="responseMessage">HttpResponseMessage as received from the remote server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual ValueTask AfterHttpRequest(HttpResponseMessage responseMessage, CancellationToken cancellationToken) => Completed;

    /// <summary>
    /// Intercepts the request after it's created from HttpResponseMessage
    /// </summary>
    /// <param name="response">HttpResponseMessage as received from the remote server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual ValueTask AfterRequest(RestResponse response, CancellationToken cancellationToken) => Completed;
    
    /// <summary>
    /// Intercepts the request before deserialization, won't be called if using non-generic ExecuteAsync
    /// </summary>
    /// <param name="response">HttpResponseMessage as received from the remote server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual ValueTask BeforeDeserialization(RestResponse response, CancellationToken cancellationToken) => Completed;
}
