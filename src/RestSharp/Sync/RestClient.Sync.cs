//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

public partial class RestClient {
    /// <summary>
    /// Executes the request synchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    public RestResponse Execute(RestRequest request) => AsyncHelpers.RunSync(() => ExecuteAsync(request));

    /// <summary>
    /// A specialized method to download files as streams.
    /// </summary>
    /// <param name="request">Pre-configured request instance.</param>
    /// <returns>The downloaded stream.</returns>
    [PublicAPI]
    public Stream? DownloadStream(RestRequest request) => AsyncHelpers.RunSync(() => DownloadStreamAsync(request));
}
