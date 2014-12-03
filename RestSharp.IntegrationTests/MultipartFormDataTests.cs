using System;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    public class MultipartFormDataTests
    {
        [Test]
        public void MultipartFormDataAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                AddParameters(request);

                client.ExecuteAsync(request, (restResponse, handle) =>
                {
                    Console.WriteLine(restResponse.Content);
                    Assert.Equal(Expected, restResponse.Content);
                });
            }
        }

        [Test]
        public void MultipartFormData()
        {
            //const string baseUrl = "http://localhost:8888/";
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                AddParameters(request);

                var response = client.Execute(request);

                //Console.WriteLine(response.Content);

#if APPVEYOR
                Assert.AreEqual(Expected, response.Content);
#else
                Assert.AreEqual(Expected, response.Content);
#endif
            }
        }

        private void AddParameters(RestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }

        private const string Expected = 
@"-------------------------------28947758029299
Content-Disposition: form-data; name=""foo""

bar
-------------------------------28947758029299
Content-Disposition: form-data; name=""a name with spaces""

somedata
-------------------------------28947758029299--
";

        private void EchoHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = 200;

            var streamReader = new System.IO.StreamReader(obj.Request.InputStream);

            obj.Response.OutputStream.WriteStringUtf8(streamReader.ReadToEnd());
        }
    }
}
