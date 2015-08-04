using System;
using System.IO;
using System.Net;
using System.Threading;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class MultipartFormDataTests
    {
        private readonly string expected =
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"foo\"" + Environment.NewLine + Environment.NewLine +
            "bar" + Environment.NewLine +
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"a name with spaces\"" + Environment.NewLine + Environment.NewLine +
            "somedata" + Environment.NewLine +
            "-------------------------------28947758029299--" + Environment.NewLine;

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
                                                 Assert.AreEqual(this.expected, restResponse.Content);
                                             });
            }
        }

        [Test]
        public void MultipartFormData()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                AddParameters(request);

                var response = client.Execute(request);

                Assert.AreEqual(this.expected, response.Content);
            }
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_Execute()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("?json_route=/posts")
                                  {
                                      AlwaysMultipartFormData = true,
                                      Method = Method.POST,
                                  };

                request.AddParameter("title", "test", ParameterType.RequestBody);

                var response = client.Execute(request);

                Assert.Null(response.ErrorException);
            }
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_ExecuteTaskAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("?json_route=/posts")
                                  {
                                      AlwaysMultipartFormData = true,
                                      Method = Method.POST,
                                  };

                request.AddParameter("title", "test", ParameterType.RequestBody);

                var task = client.ExecuteTaskAsync(request).ContinueWith(x => { Assert.Null(x.Result.ErrorException); });

                task.Wait();
            }
        }

        [Test]
        public void AlwaysMultipartFormData_WithParameter_ExecuteAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("?json_route=/posts")
                              {
                                  AlwaysMultipartFormData = true,
                                  Method = Method.POST,
                              };

                request.AddParameter("title", "test", ParameterType.RequestBody);

                IRestResponse syncResponse = null;

                using (var eventWaitHandle = new AutoResetEvent(false))
                {
                    client.ExecuteAsync(request, response =>
                                                 {
                                                     syncResponse = response;
                                                     eventWaitHandle.Set();
                                                 });

                    eventWaitHandle.WaitOne();
                }

                Assert.Null(syncResponse.ErrorException);
            }
        }

        private static void EchoHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = 200;

            var streamReader = new StreamReader(obj.Request.InputStream);

            obj.Response.OutputStream.WriteStringUtf8(streamReader.ReadToEnd());
        }

        private static void AddParameters(IRestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }
    }
}