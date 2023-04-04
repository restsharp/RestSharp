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

using RestSharp.Serializers;

namespace RestSharp;

public interface IRestClient : IDisposable {
    /// <summary>
    /// Client options that aren't used for configuring HttpClient
    /// </summary>
    ReadOnlyRestClientOptions Options { get; }

    /// <summary>
    /// Client-level serializers
    /// </summary>
    RestSerializers Serializers { get; }

    /// <summary>
    /// Default parameters to use on every request made with this client instance.
    /// </summary>
    DefaultParameters DefaultParameters { get; }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// A specialized method to download files as streams.
    /// </summary>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The downloaded stream.</returns>
    Task<Stream?> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default);
}
