using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    class ParameterTest
    {
        private SimpleServer _server;
        private const string BASE_URL = "http://localhost:8888/";

        [SetUp]
        public void SetupServer()
        {
            _server = SimpleServer.Create(BASE_URL, RequestHandler.Handle);
        }

        [TearDown]
        public void DisposeServer()
        {
            _server.Dispose();
        }

        [Test(Description = "Parameters with null instead of name should not be allowed.")]
        public void Parameters_with_null_name_not_allowed()
        {
            var client = new RestClient(BASE_URL + "{foo}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
            var request = new RestRequest("{foo1}").AddParameter(
                new Parameter(null, "bar1", ParameterType.UrlSegment));
            var response = client.Execute(request);
            Assert.AreEqual(response.ResponseStatus, ResponseStatus.Error);
            Assert.IsInstanceOf<ArgumentNullException>(response.ErrorException);
            Assert.AreEqual("Parameter name is missing. Please make sure that all the parameters names are not null.\r\nParameter name: null", response.ErrorMessage);
        }

        private static class RequestHandler
        {
            public static Uri Url { get; private set; }

            public static void Handle(HttpListenerContext context)
            {
                Url = context.Request.Url;
                Handlers.Echo(context);
            }

        }
    }
}
