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

namespace RestSharp.Tests.Integrated;

using Server;

public class RedirectTests : IDisposable {
    readonly WireMockServer _server = WireMockTestServer.StartTestServer();
    readonly RestClient     _client;

    public RedirectTests() => _client = new RestClient(new RestClientOptions(_server.Url!) { FollowRedirects = true });

    [Fact]
    public async Task Can_Perform_GET_Async_With_Redirect() {
        const string val = "Works!";

        var request = new RestRequest("redirect");

        var response = await _client.ExecuteAsync<SuccessResponse>(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data!.Message.Should().Be(val);
    }

    public void Dispose() {
        _server.Dispose();
        _client.Dispose();
    }
}