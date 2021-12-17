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

public static partial class RestClientExtensions {
    public static Task<TResponse?> PostAsync<TRequest, TResponse>(
        this RestClient   client,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest().AddJsonBody(request);
        return client.PostAsync<TResponse>(restRequest, cancellationToken);
    }
    
    public static Task<TResponse?> PutAsync<TRequest, TResponse>(
        this RestClient   client,
        TRequest          request,
        CancellationToken cancellationToken = default
    ) where TRequest : class {
        var restRequest = new RestRequest().AddJsonBody(request);
        return client.PutAsync<TResponse>(restRequest, cancellationToken);
    }
}