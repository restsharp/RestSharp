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

using Moq;
using RestSharp.Tests.Integrated.Server;

namespace RestSharp.Tests.Integrated.Interceptor; 

[Collection(nameof(TestServerCollection))]
public class InterceptorTests {
    readonly RestClient _client;

    public InterceptorTests(TestServerFixture fixture) => _client = new RestClient(fixture.Server.Url);

    [Fact]
    public async Task AddInterceptor_ShouldBeUsed() {
        //Arrange
        var body            = new TestRequest("foo", 100);
        var request         = new RestRequest("post/json").AddJsonBody(body);
        
        var mockInterceptor = new Mock<Interceptors.Interceptor>();
        var interceptor     = mockInterceptor.Object;
        var options         = _client.Options;
        options.Interceptors.Add(interceptor);
        //Act
        var response = await _client.ExecutePostAsync<TestResponse>(request);
        //Assert
        mockInterceptor.Verify(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>()));
        mockInterceptor.Verify(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>()));
        mockInterceptor.Verify(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>()));
        mockInterceptor.Verify(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>()));
    }
    [Fact]
    public async Task ThrowExceptionIn_InterceptBeforeSerialization_ShouldBeCatchedInTest() {
        //Arrange
        var body    = new TestRequest("foo", 100);
        var request = new RestRequest("post/json").AddJsonBody(body);
        
        var mockInterceptor = new Mock<Interceptors.Interceptor>();
        mockInterceptor.Setup(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>())).Throws<Exception>(() => throw new Exception("DummyException"));
        var interceptor     = mockInterceptor.Object;
        var options         = _client.Options;
        options.Interceptors.Add(interceptor);
        //Act
        var action = () => _client.ExecutePostAsync<TestResponse>(request);
        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        mockInterceptor.Verify(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>()));
        mockInterceptor.Verify(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>()),Times.Never);
        mockInterceptor.Verify(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>()),Times.Never);
        mockInterceptor.Verify(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>()),Times.Never);
    }
    [Fact]
    public async Task ThrowExceptionIn_InterceptBeforeRequest_ShouldBeCatchableInTest() {
        //Arrange
        var body    = new TestRequest("foo", 100);
        var request = new RestRequest("post/json").AddJsonBody(body);
        
        var mockInterceptor = new Mock<Interceptors.Interceptor>();
        mockInterceptor.Setup(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>())).Throws<Exception>(() => throw new Exception("DummyException"));
        var interceptor = mockInterceptor.Object;
        var options     = _client.Options;
        options.Interceptors.Add(interceptor);
        //Act
        var action = () => _client.ExecutePostAsync<TestResponse>(request);
        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        mockInterceptor.Verify(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>()));
        mockInterceptor.Verify(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>()));
        mockInterceptor.Verify(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>()),Times.Never);
        mockInterceptor.Verify(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>()),Times.Never);
    }
    [Fact]
    public async Task ThrowExceptionIn_InterceptAfterRequest_ShouldBeCatchableInTest() {
        //Arrange
        var body    = new TestRequest("foo", 100);
        var request = new RestRequest("post/json").AddJsonBody(body);
        
        var mockInterceptor = new Mock<Interceptors.Interceptor>();
        mockInterceptor.Setup(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>())).Throws<Exception>(() => throw new Exception("DummyException"));
        var interceptor = mockInterceptor.Object;
        var options     = _client.Options;
        options.Interceptors.Add(interceptor);
        //Act
        var action = () => _client.ExecutePostAsync<TestResponse>(request);
        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        mockInterceptor.Verify(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>()));
        mockInterceptor.Verify(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>()));
        mockInterceptor.Verify(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>()));
        mockInterceptor.Verify(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>()),Times.Never);
    }
    [Fact]
    public async Task ThrowException_InInterceptBeforeDeserialize_ShouldBeCatchableInTest() {
        //Arrange
        var body    = new TestRequest("foo", 100);
        var request = new RestRequest("post/json").AddJsonBody(body);
        
        var mockInterceptor = new Mock<Interceptors.Interceptor>();
        mockInterceptor.Setup(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>())).Throws<Exception>(() => throw new Exception("DummyException"));
        var interceptor = mockInterceptor.Object;
        var options     = _client.Options;
        options.Interceptors.Add(interceptor);
        //Act
        var action = () => _client.PostAsync<TestResponse>(request);
        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("DummyException");
        mockInterceptor.Verify(m => m.InterceptBeforeSerialization(It.IsAny<RestRequest>()));
        mockInterceptor.Verify(m => m.InterceptBeforeRequest(It.IsAny<HttpRequestMessage>()));
        mockInterceptor.Verify(m => m.InterceptAfterRequest(It.IsAny<HttpResponseMessage>()));
        mockInterceptor.Verify(m => m.InterceptBeforeDeserialize(It.IsAny<RestResponse>()));
    }
    
    
}