//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Extensions
{
    public static class WebRequestExtensions
    {
        public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest, CancellationToken cancellationToken)
            => Task.Run(
                () =>
                    Task<Stream>.Factory.FromAsync(
                        (callback, state) => ((WebRequest) state).BeginGetRequestStream(callback, state),
                        iar => ((WebRequest) iar.AsyncState).EndGetRequestStream(iar),
                        webRequest
                    ), cancellationToken
            );

        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest, CancellationToken cancellationToken)
            => Task.Run(
                () =>
                    Task<WebResponse>.Factory.FromAsync(
                        (callback, state) => ((WebRequest) state).BeginGetResponse(callback, state),
                        iar => ((WebRequest) iar.AsyncState).EndGetResponse(iar),
                        webRequest
                    ), cancellationToken
            );
    }
}