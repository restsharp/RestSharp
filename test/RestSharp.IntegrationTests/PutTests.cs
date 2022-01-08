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

using System.Text.Json;
using RestSharp.IntegrationTests.Fixtures;

namespace RestSharp.IntegrationTests; 

[Collection(nameof(TestServerCollection))]
public class PutTests {
    readonly ITestOutputHelper _output;
    readonly RestClient        _client;

    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public PutTests(TestServerFixture fixture, ITestOutputHelper output) {
        _output  = output;
        _client  = new RestClient(fixture.Server.Url);
    }

    [Fact]
    public async Task Should_put_json_body() {
        var body     = new TestRequest("foo", 100);
        var request  = new RestRequest("content").AddJsonBody(body);
        var response = await _client.PutAsync(request);

        var expected = JsonSerializer.Serialize(body, Options);
        response!.Content.Should().Be(expected);
    }

    [Fact]
    public async Task Should_put_json_body_using_extension() {
        var body     = new TestRequest("foo", 100);
        var response = await _client.PutJsonAsync<TestRequest, TestRequest>("content", body);
        response.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Can_Timeout_PUT_Async() {
        var request = new RestRequest("timeout", Method.Put).AddBody("Body_Content");

        // Half the value of ResponseHandler.Timeout
        request.Timeout = 200;

        var response = await _client.ExecuteAsync(request);

        Assert.Equal(ResponseStatus.TimedOut, response.ResponseStatus);
    }
    
}

record TestRequest(string Data, int Number);