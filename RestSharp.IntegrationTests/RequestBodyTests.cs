using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class RequestBodyTests
    {
        private const string BASE_URL = "http://localhost:8888/";

        [Test]
        public void Can_Not_Be_Added_To_GET_Request()
        {
            const Method httpMethod = Method.GET;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Have_No_Body_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                client.Execute(request);

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Be_Added_To_POST_Request()
        {
            const Method httpMethod = Method.POST;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_PUT_Request()
        {
            const Method httpMethod = Method.PUT;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_DELETE_Request()
        {
            const Method httpMethod = Method.DELETE;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Not_Be_Added_To_HEAD_Request()
        {
            const Method httpMethod = Method.HEAD;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasNoRequestBody();
            }
        }

        [Test]
        public void Can_Be_Added_To_OPTIONS_Request()
        {
            const Method httpMethod = Method.OPTIONS;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_PATCH_Request()
        {
            const Method httpMethod = Method.PATCH;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void Can_Be_Added_To_COPY_Request()
        {
            const Method httpMethod = Method.COPY;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);
                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";

                request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

                client.Execute(request);

                AssertHasRequestBody(contentType, bodyData);
            }
        }

        [Test]
        public void MultipartFormData_Without_File_Creates_A_Valid_RequestBody()
        {
            string expectedFormBoundary = "-------------------------------28947758029299";


            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                RestClient client = new RestClient(BASE_URL);

                RestRequest request = new RestRequest(RequestBodyCapturer.RESOURCE, Method.POST)
                {
                    AlwaysMultipartFormData = true
                };

                const string contentType = "text/plain";
                const string bodyData = "abc123 foo bar baz BING!";
                const string multipartName = "mybody";

                request.AddParameter(multipartName, bodyData, contentType, ParameterType.RequestBody);

                client.Execute(request);

                string expectedBody = expectedFormBoundary +
                                      Environment.NewLine
                                      + "Content-Type: " +
                                      contentType
                                      + Environment.NewLine
                                      + @"Content-Disposition: form-data; name=""" + multipartName + @""""
                                      + Environment.NewLine
                                      + Environment.NewLine
                                      + bodyData
                                      + Environment.NewLine
                                      + expectedFormBoundary + "--"
                                      + Environment.NewLine;

                Console.WriteLine(RequestBodyCapturer.CapturedEntityBody);
                Assert.AreEqual(expectedBody, RequestBodyCapturer.CapturedEntityBody, "Empty multipart generated: " + RequestBodyCapturer.CapturedEntityBody);
            }
        }

        [Test]
        public void Query_Parameters_With_Json_Body()
        {
            const Method httpMethod = Method.PUT;

            using (SimpleServer.Create(BASE_URL, Handlers.Generic<RequestBodyCapturer>()))
            {
                var client = new RestClient(BASE_URL);
                var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod)
                    .AddJsonBody(new {displayName = "Display Name"})
                    .AddQueryParameter("key", "value");

                client.Execute(request);

                Assert.AreEqual("http://localhost:8888/Capture?key=value", RequestBodyCapturer.CapturedUrl.ToString());
                Assert.AreEqual("application/json", RequestBodyCapturer.CapturedContentType);
                Assert.AreEqual("{\"displayName\":\"Display Name\"}", RequestBodyCapturer.CapturedEntityBody);
            }
        }
        
        private static void AssertHasNoRequestBody()
        {
            Assert.Null(RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(false, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(string.Empty, RequestBodyCapturer.CapturedEntityBody);
        }

        private static void AssertHasRequestBody(string contentType, string bodyData)
        {
            Assert.AreEqual(contentType, RequestBodyCapturer.CapturedContentType);
            Assert.AreEqual(true, RequestBodyCapturer.CapturedHasEntityBody);
            Assert.AreEqual(bodyData, RequestBodyCapturer.CapturedEntityBody);
        }

        private class RequestBodyCapturer
        {
            public const string RESOURCE = "Capture";

            public static string CapturedContentType { get; set; }

            public static bool CapturedHasEntityBody { get; set; }

            public static string CapturedEntityBody { get; set; }

            public static Uri CapturedUrl { get; set; }
            
            public static void Capture(HttpListenerContext context)
            {
                HttpListenerRequest request = context.Request;

                CapturedContentType = request.ContentType;
                CapturedHasEntityBody = request.HasEntityBody;
                CapturedEntityBody = StreamToString(request.InputStream);
                CapturedUrl = request.Url;
            }
            
            private static string StreamToString(Stream stream)
            {
                StreamReader streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
