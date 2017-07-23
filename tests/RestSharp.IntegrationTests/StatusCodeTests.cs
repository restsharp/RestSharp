using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StatusCodeTests
    {
        [Test]
        public void Handles_GET_Request_404_Error()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, UrlToStatusCodeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("404");
                IRestResponse response = client.Execute(request);

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Test]
        public void Handles_GET_Request_404_Error_With_Body()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, UrlToStatusCodeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("404");

                request.AddBody("This is the body");

                IRestResponse response = client.Execute(request);

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        private static void UrlToStatusCodeHandler(HttpListenerContext obj)
        {
            obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }

        [Test]
        public void Handles_Different_Root_Element_On_Http_Error()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("error")
                                      {
                                          RootElement = "Success"
                                      };

                request.OnBeforeDeserialization = resp =>
                                                  {
                                                      if (resp.StatusCode == HttpStatusCode.BadRequest)
                                                      {
                                                          request.RootElement = "Error";
                                                      }
                                                  };

                IRestResponse<Response> response = client.Execute<Response>(request);

                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.AreEqual("Not found!", response.Data.Message);
            }
        }

        [Test]
        public void Handles_Default_Root_Element_On_No_Error()
        {
            Uri baseUrl = new Uri("http://localhost:8888/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, Handlers.Generic<ResponseHandler>()))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("success")
                                      {
                                          RootElement = "Success"
                                      };

                request.OnBeforeDeserialization = resp =>
                                                  {
                                                      if (resp.StatusCode == HttpStatusCode.NotFound)
                                                      {
                                                          request.RootElement = "Error";
                                                      }
                                                  };

                IRestResponse<Response> response = client.Execute<Response>(request);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Works!", response.Data.Message);
            }
        }
    }

    public class ResponseHandler
    {
        private void error(HttpListenerContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.Headers.Add("Content-Type", "application/xml");
            context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>");
        }

        private void errorwithbody(HttpListenerContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.Headers.Add("Content-Type", "application/xml");
            context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Error>
        <Message>Not found!</Message>
    </Error>
</Response>");
        }

        private void success(HttpListenerContext context)
        {
            context.Response.OutputStream.WriteStringUtf8(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
    <Success>
        <Message>Works!</Message>
    </Success>
</Response>");
        }
    }

    public class Response
    {
        public string Message { get; set; }
    }
}
