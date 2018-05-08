using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class ProxyTests
    {
        private const string BASE_URL_SERVER = "http://localhost:8888/";
        private string BASE_URL_CLIENT;

        public ProxyTests()
        {
            BASE_URL_CLIENT = $"http://{Environment.MachineName}:8888/";
        }

        [Test]
        public void Set_Invalid_Proxy_Fails()
        {
            const Method httpMethod = Method.GET;

            using (SimpleServer.Create(BASE_URL_SERVER, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL_CLIENT);
                client.Proxy = new WebProxy("non_existent_proxy", false);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);
                var response = client.Execute(request);
                Assert.False(response.IsSuccessful);
                Assert.IsInstanceOf<WebException>(response.ErrorException);

#if NETCORE
                Assert.AreEqual("An error occurred while sending the request. The server name or address could not be resolved", response.ErrorMessage);
#endif

#if !NETCORE
                Assert.AreEqual("The proxy name could not be resolved: 'non_existent_proxy'", response.ErrorMessage);
#endif
            }
        }

        [Test]
        public void Set_Invalid_Proxy_Fails_RAW()
        {
            const Method httpMethod = Method.GET;

            using (SimpleServer.Create(BASE_URL_SERVER, Handlers.Generic<RequestBodyCapturer>()))
            {
                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";
                var requestUri = new Uri(new Uri(BASE_URL_CLIENT), RequestBodyCapturer.RESOURCE);
                var webRequest = (HttpWebRequest) WebRequest.Create(requestUri);
                webRequest.Proxy = new WebProxy("non_existent_proxy", false);
                //webRequest.Proxy = new WebProxy("non_existing", false);
                // webRequest.Proxy = HttpWebRequest.DefaultWebProxy;
               
                Assert.Throws<WebException>(() => webRequest.GetResponse());

                // Assert.False(response.IsSuccessful);
                // Assert.IsInstanceOf<WebException>(response.ErrorException);
                // Assert.AreEqual("The proxy name could not be resolved: 'non_existent_proxy'", response.ErrorMessage);
            }
        }

        private class RequestBodyCapturer
        {
            public const string RESOURCE = "Capture";

            public static string CapturedContentType { get; set; }

            public static bool CapturedHasEntityBody { get; set; }

            public static string CapturedEntityBody { get; set; }

            public static void Capture(HttpListenerContext context)
            {
                HttpListenerRequest request = context.Request;

                CapturedContentType = request.ContentType;
                CapturedHasEntityBody = request.HasEntityBody;
                CapturedEntityBody = StreamToString(request.InputStream);
            }

            private static string StreamToString(Stream stream)
            {
                StreamReader streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
