using NUnit.Framework;
using Shouldly;

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

            req.Method.ShouldBe(overrideMethod);
        }

        [Test]
        public void ConfigureHttp_will_set_proxy_to_null_with_no_exceptions_When_no_proxy_can_be_found()
        {
            var req    = new RestRequest();
            var client = new RestClient(BaseUrl) {Proxy = null};

            Should.NotThrow(() => client.Execute(req));
            client.Proxy.ShouldBeNull();
        }
    }
}