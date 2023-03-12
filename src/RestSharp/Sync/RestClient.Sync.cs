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

public static partial class RestClientExtensions {
    /// <summary>
    /// Executes the request synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public static RestResponse Execute(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, cancellationToken));

    /// <summary>
    /// A specialized method to download files as streams.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The downloaded stream.</returns>
    [PublicAPI]
    public static Stream? DownloadStream(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => AsyncHelpers.RunSync(() => client.DownloadStreamAsync(request, cancellationToken));
}
