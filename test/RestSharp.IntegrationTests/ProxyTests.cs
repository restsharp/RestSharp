using System;
using System.Net;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class ProxyTests
    {
        class RequestBodyCapturer
        {
            public const string RESOURCE = "Capture";
        }

        [Test]
        public void Set_Invalid_Proxy_Fails()
        {
            using var server = HttpServerFixture.StartServer((_, __) => { });

            var client = new RestClient(server.Url) {Proxy = new WebProxy("non_existent_proxy", false)};
            var request = new RestRequest();

            const string contentType = "text/plain";
            const string bodyData    = "abc123 foo bar baz BING!";

            request.AddParameter(contentType, bodyData, ParameterType.RequestBody);
            var response = client.Get(request);
            
            Assert.False(response.IsSuccessful);
            Assert.IsInstanceOf<WebException>(response.ErrorException);

#if NETCORE
            Assert.AreEqual(WebExceptionStatus.NameResolutionFailure, ((WebException)response.ErrorException).Status);
#else
                Assert.AreEqual(
                    WebExceptionStatus.ProxyNameResolutionFailure,
                    ((WebException) response.ErrorException).Status
                );
#endif
        }

        [Test]
        public void Set_Invalid_Proxy_Fails_RAW()
        {
            using var server = HttpServerFixture.StartServer((_, __) => { });

            var requestUri = new Uri(new Uri(server.Url), "");
            var webRequest = (HttpWebRequest) WebRequest.Create(requestUri);
            webRequest.Proxy = new WebProxy("non_existent_proxy", false);

            Assert.Throws<WebException>(() => webRequest.GetResponse());
        }
    }
}