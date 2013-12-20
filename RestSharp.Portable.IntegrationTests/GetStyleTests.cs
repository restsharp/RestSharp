using System.Linq;
using System.Threading;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;
using System;
using System.Net.Http.Formatting;
using System.Web;

namespace RestSharp.IntegrationTests
{
    [Trait("Integration", "GET Style Tests")]
	public class GetStyleTests
	{
        const string baseUrl = "http://localhost:8080/";
        
        [Fact]
        
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
                Assert.Equal(content, response.Content);
			}
		}

        [Fact]
        
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
                Assert.Equal("John", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }

        [Fact]
        
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

                Assert.Equal("/users/123", response.ResponseUri.AbsolutePath);
                Assert.NotNull(response.Content);
                Assert.Equal(content, response.Content);
            }
        }

        [Fact]
        
        public async void Can_Execute_GET_With_Cookie_Request()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoCookieRequestValue("ThisIsATestCookie")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");
                request.AddCookie("ThisIsATestCookie", "YummyCookies");

                var response = await client.ExecuteAsync(request);

                Assert.NotNull(response.Content);
                Assert.Equal(response.Content, "YummyCookies");
            }
        }

        [Fact]
        
        public async void Can_Execute_GET_With_Cookie_Response()
        {            
            using (SimpleServer.Create(baseUrl, Handlers.EchoCookieResponseValue("ThisIsATestCookie", "YummyCookies")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                var response = await client.ExecuteAsync(request);

                Assert.NotNull(response.Cookies);
                Assert.Equal(response.Cookies[0].Value, "YummyCookies");
            }
        }

        [Fact]
        
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
                Assert.Equal(nvc["ThisIsANewParameter"], "This Is The Parameter Value");
            }
        }

        [Fact]
        
        public async void Can_Execute_GET_With_Accept_Header()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoMediaTypeValue("text/csv")))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/");
                request.AddHeader("Accept", "text/csv");

                var response = await client.ExecuteAsync(request);

                Assert.Equal("text/csv", response.ContentType.MediaType);
            }
        }

        [Fact]
        
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
                Assert.Equal("John", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }

        [Fact]
        
        public async void Can_Handle_GET_Protocol_Error()
        {
            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/NotFound/404");

                var response = await client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal("NotFound", response.StatusDescription);
            }
        }

        [Fact]
        
        public async void Can_Handle_GET_Protocol_Error_With_Body()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/BadRequestErrorWithBody");
                var response = await client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("Bad Request", response.StatusDescription);
                Assert.NotNull(response.Content);
                Assert.Equal(ResponseHandler.BadRequestErrorContent, response.Content);
            }
        }

        [Fact]
        
        public async void Can_Handle_GET_Generic_Protocol_Error()
        {
            using (SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/BadRequest/400");
                var response = await client.ExecuteAsync<Response>(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("BadRequest", response.StatusDescription);
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
