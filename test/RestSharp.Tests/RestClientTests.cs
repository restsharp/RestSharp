using System;
using FluentAssertions;
using NUnit.Framework;

namespace RestSharp.Tests
{
    public class RestClientTests
    {
        const string BaseUrl = "http://localhost:8888/";

        [Test]
        [TestCase(Method.GET, Method.POST)]
        [TestCase(Method.POST, Method.GET)]
        [TestCase(Method.DELETE, Method.GET)]
        [TestCase(Method.HEAD, Method.POST)]
        [TestCase(Method.PUT, Method.PATCH)]
        [TestCase(Method.PATCH, Method.PUT)]
        [TestCase(Method.POST, Method.PUT)]
        [TestCase(Method.GET, Method.DELETE)]
        public void Execute_with_IRestRequest_and_Method_overrides_previous_request_method(Method reqMethod, Method overrideMethod)
        {
            var req    = new RestRequest(reqMethod);
            var client = new RestClient(BaseUrl);

            client.Execute(req, overrideMethod);

            req.Method.Should().Be(overrideMethod);
        }

        [Test]
        public void ConfigureHttp_will_set_proxy_to_null_with_no_exceptions_When_no_proxy_can_be_found()
        {
            var req    = new RestRequest();
            var client = new RestClient(BaseUrl) {Proxy = null};

            Assert.DoesNotThrow(() => client.Execute(req));
            client.Proxy.Should().BeNull();
        }
        
        [Test]
        public void BuildUri_should_build_with_passing_link_as_Uri()
        {
            // arrange
            var relative = new Uri("/foo/bar/baz", UriKind.Relative);
            var absoluteUri = new Uri(new Uri(BaseUrl), relative);
            var req = new RestRequest(absoluteUri);
            
            // act
            var client = new RestClient();
            var builtUri = client.BuildUri(req);
            
            // assert
            absoluteUri.Should().Be(builtUri);
        }
        
        [Test]
        public void BuildUri_should_build_with_passing_link_as_Uri_with_set_BaseUrl()
        {
            // arrange
            var baseUrl = new Uri(BaseUrl);
            var relative = new Uri("/foo/bar/baz", UriKind.Relative);
            var req = new RestRequest(relative);
            
            // act
            var client = new RestClient(baseUrl);
            var builtUri = client.BuildUri(req);
            
            // assert
            new Uri(baseUrl, relative).Should().Be(builtUri);
        }
    }
}