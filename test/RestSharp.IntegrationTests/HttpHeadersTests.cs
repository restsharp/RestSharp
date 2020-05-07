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

using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.IntegrationTests.Fixtures;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class HttpHeadersTests : CaptureFixture
    {
        [Test]
        public void Ensure_headers_correctly_set_in_the_hook()
        {
            const string headerName  = "HeaderName";
            const string headerValue = "HeaderValue";

            using var server = SimpleServer.Create(Handlers.Generic<RequestHeadCapturer>());

            // Prepare
            var client = new RestClient(server.Url);

            var request = new RestRequest(RequestHeadCapturer.Resource)
            {
                OnBeforeRequest = http => http.Headers.Add(new HttpHeader(headerName, headerValue))
            };

            // Run
            client.Execute(request);

            // Assert
            RequestHeadCapturer.CapturedHeaders[headerName].Should().Be(headerValue);
        }
    }
}
