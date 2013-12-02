using System.Linq;
using System.Threading;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
//using Xunit;
using NUnit;
using System;
using System.Net.Http.Formatting;
using System.Web;
using NUnit.Framework;

namespace RestSharp.IntegrationTests
{
    //[Trait("Integration", "GET Style Tests")]
    [Category("Integation [GET Style Tests]")]
	public class GetStyleTests
	{
        const string baseUrl = "http://localhost:8080/";
        
        //[Fact]
        [Test]
		public async void Can_Execute_GET()
		{
            string content = "{\"first\":\"John\", \"last\":\"Doe\"}";
                        
            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(content)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("users/123");
                
                var response = await client.ExecuteAsync(request);

                Console.WriteLine(response);

                Assert.NotNull(response.Content);
                Assert.AreEqual(content, response.Content);
			}
		}

        //[Fact]
        [Test]
        public async void Can_Execute_GET_Generic()
        {
            string content = "{\"first\":\"John\", \"last\":\"Doe\"}";

            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(content)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                var response = await client.ExecuteAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.AreEqual("John", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GET_With_UrlSegment()
        {
            string content = "{\"first\":\"John\", \"last\":\"Doe\"}";

            using (SimpleServer.Create(baseUrl, Handlers.EchoSegmentValue("123")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/{account}");
                request.AddUrlSegment("account", "123");

                var response = await client.ExecuteAsync(request);

                Console.WriteLine(response);

                Assert.AreEqual("/users/123", response.ResponseUri.AbsolutePath);
                Assert.NotNull(response.Content);
                Assert.AreEqual(content, response.Content);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GET_With_Cookie_Request()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoCookieRequestValue("ThisIsATestCookie")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");
                request.AddCookie("ThisIsATestCookie", "YummyCookies");

                var response = await client.ExecuteAsync(request);

                Assert.NotNull(response.Content);
                Assert.AreEqual(response.Content, "YummyCookies");
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GET_With_Cookie_Response()
        {            
            using (SimpleServer.Create(baseUrl, Handlers.EchoCookieResponseValue("ThisIsATestCookie", "YummyCookies")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                var response = await client.ExecuteAsync(request);

                Assert.NotNull(response.Cookies);
                Assert.AreEqual(response.Cookies[0].Value, "YummyCookies");
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GET_With_Querystring_Parameters()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoQuerystringValue("ThisIsANewParameter")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/{account}");
                request.AddUrlSegment("account", "123");
                request.AddParameter("ThisIsANewParameter", "This Is The Parameter Value");

                var response = await client.ExecuteAsync(request);
                var nvc = HttpUtility.ParseQueryString(response.ResponseUri.Query);
                
                Assert.NotNull(nvc["ThisIsANewParameter"]);
                Assert.AreEqual(nvc["ThisIsANewParameter"], "This Is The Parameter Value");
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GET_With_Accept_Header()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoMediaTypeValue("text/csv")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/");
                request.AddHeader("Accept", "text/csv");

                var response = await client.ExecuteAsync(request);

                Assert.AreEqual("text/csv", response.ContentType.MediaType);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_GetAsync()
        {
            string content = "{\"first\":\"John\", \"last\":\"Doe\"}";

            using (SimpleServer.Create(baseUrl, Handlers.EchoValue(content)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                var response = await client.ExecuteGetAsync<Name>(request);
                
                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.AreEqual("John", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Handle_GET_Protocol_Error()
        {
            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/NotFound/404");

                var response = await client.ExecuteAsync(request);

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                Assert.AreEqual("NotFound", response.StatusDescription);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Handle_GET_Protocol_Error_With_Body()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/BadRequestErrorWithBody");
                var response = await client.ExecuteAsync(request);

                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.AreEqual("Bad Request", response.StatusDescription);
                Assert.NotNull(response.Content);
                Assert.AreEqual(ResponseHandler.BadRequestErrorContent, response.Content);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Handle_GET_Generic_Protocol_Error()
        {
            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/BadRequest/400");
                var response = await client.ExecuteAsync<Response>(request);

                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.AreEqual("BadRequest", response.StatusDescription);
            }
        }


        //[Fact]
        //public void Can_Perform_GetTaskAsync_With_Response_Type()
        //{
        //    const string baseUrl = "http://localhost:8080/";
        //    using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
        //    {
        //        var client = new RestClient(baseUrl);
        //        var request = new RestRequest("success");

        //        //var task = client.GetAsync<Response>(request);
        //        //task.Wait();

        //        //Assert.AreEqual("Works!", task.Result.Message);
        //    }
        //}

        //[Fact]
        //public async void Can_Perform_GET_With_Response_Type()
        //{
        //    using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
        //    {
        //        var client = new RestClient(baseUrl);
        //        var request = new RestRequest("success");

        //        var response = await client.ExecuteAsync<Response>(request);

        //        Assert.AreEqual("Works!", response.Data.Message);
        //    }
        //}


        //[Fact]
        //public void Can_Cancel_GET()
        //{
        //    const string val = "Basic async task test";

        //    using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
        //    {
        //        var client = new RestClient(baseUrl);
        //        var request = new RestRequest("timeout");
        //        var cancellationTokenSource = new CancellationTokenSource();

        //        var task = client.ExecuteAsync(request, cancellationTokenSource.Token);
        //        cancellationTokenSource.Cancel();

        //        Assert.True(task.IsCanceled);
        //    }
        //}

        //[Fact]
        //public void Can_Cancel_GET_With_Response_Type()
        //{
        //    const string val = "Basic async task test";
        //    using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
        //    {
        //        var client = new RestClient(baseUrl);
        //        var request = new RestRequest("timeout");
        //        var cancellationTokenSource = new CancellationTokenSource();

        //        var task = client.ExecuteAsync<Response>(request, cancellationTokenSource.Token);
        //        cancellationTokenSource.Cancel();

        //        Assert.True(task.IsCanceled);
        //    }
        //}


		private void UrlToStatusCodeHandler(HttpListenerContext obj)
		{
            var code = Enum.Parse(typeof(HttpStatusCode), obj.Request.Url.Segments.Last());

			obj.Response.StatusCode = (int)code;
            obj.Response.StatusDescription = code.ToString();
		}

		public class ResponseHandler
		{
            public static string BadRequestErrorContent = @"<?xml version=""1.0"" encoding=""utf-8"" ?><Response><Error><Message>Not found!</Message></Error></Response>";
			public void BadRequestError(HttpListenerContext context)
			{
				context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
			}
            public void BadRequestErrorWithBody(HttpListenerContext context)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.Headers.Add("Content-Type", "application/xml");
                context.Response.OutputStream.WriteStringUtf8(BadRequestErrorContent);
            }
            
            public void success(HttpListenerContext context)
			{
				context.Response.OutputStream.WriteStringUtf8(
	@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Response>
	<Success>
		<Message>Works!</Message>
	</Success>
</Response>");
			}
			void timeout(HttpListenerContext context)
			{
				Thread.Sleep(1000);
			}
		}

		public class Response
		{
			public string Message { get; set; }
		}
	}
}
