using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RestSharp.Tests
{
    public class RestClientTests
    {
        public RestClientTests() { }

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
            RestRequest req = new RestRequest(reqMethod);

            IRestClient client = new RestClient("http://localhost:8888/");

            IRestResponse res = client.Execute(req, overrideMethod);

            Assert.AreEqual(req.Method, overrideMethod);
        }
        
    }
}
