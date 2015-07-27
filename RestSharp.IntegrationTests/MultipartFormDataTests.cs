﻿namespace RestSharp.IntegrationTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    using RestSharp.IntegrationTests.Helpers;

    using Xunit;

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

        private readonly string expected_FileAndBody_RequestContent =
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Type: application/json" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"controlName\"" + Environment.NewLine + Environment.NewLine +
            "test" + Environment.NewLine +
            "-------------------------------28947758029299" + Environment.NewLine +
            "Content-Disposition: form-data; name=\"fileName\"; filename=\"TestFile.txt\"" + Environment.NewLine +
            "Content-Type: application/octet-stream" + Environment.NewLine + Environment.NewLine +
            "This is a test file for RestSharp." + Environment.NewLine +
            "-------------------------------28947758029299--" + Environment.NewLine;

        [Fact]
        public void MultipartFormData_WithParameterAndFile_Async()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };
                string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Assets\\TestFile.txt");
                request.AddFile("fileName", path);
                request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

                var task = client.ExecuteTaskAsync(request).ContinueWith(x =>
                {
                    Assert.Equal(this.expected_FileAndBody_RequestContent, x.Result.Content);
                });
                task.Wait();
            }
        }

        [Fact]
        public void MultipartFormData_WithParameterAndFile()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };
                string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Assets\\TestFile.txt");

                request.AddFile("fileName", path);
                request.AddParameter("controlName", "test", "application/json", ParameterType.RequestBody);

                var response = client.Execute(request);

                Assert.Equal(this.expected_FileAndBody_RequestContent, response.Content);
            }
        }

        [Fact]
        public void MultipartFormDataAsync()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                this.AddParameters(request);

                client.ExecuteAsync(request, (restResponse, handle) =>
                {
                    Console.WriteLine(restResponse.Content);
                    Assert.Equal(this.expected, restResponse.Content);
                });
            }
        }

        [Fact]
        public void MultipartFormData()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/", Method.POST) { AlwaysMultipartFormData = true };

                this.AddParameters(request);

                var response = client.Execute(request);

                Assert.Equal(this.expected, response.Content);
            }
        }

        [Fact]
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

        [Fact]
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

                var task = client.ExecuteTaskAsync(request).ContinueWith(
                    x =>
                        {
                            Assert.Null(x.Result.ErrorException);
                        });

                task.Wait();
            }
        }

        [Fact]
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
                    client.ExecuteAsync(
                        request,
                        response =>
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

        private void AddParameters(RestRequest request)
        {
            request.AddParameter("foo", "bar");
            request.AddParameter("a name with spaces", "somedata");
        }
    }
}