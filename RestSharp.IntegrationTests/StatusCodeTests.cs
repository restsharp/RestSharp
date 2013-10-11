using System.Linq;
using RestSharp.IntegrationTests.Helpers;
using Xunit;
using System.Net;

namespace RestSharp.IntegrationTests
{
	public class StatusCodeTests
	{
		[Fact]
		public void Handles_GET_Request_404_Error()
		{
			const string baseUrl = "http://localhost:8080/";
			using(SimpleServer.Create(baseUrl, UrlToStatusCodeHandler))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("404");
				var response = client.Execute(request);

				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

        [Fact]
        public void Handles_GET_Request_404_Error_With_Body()
        {
            const string baseUrl = "http://localhost:8080/";
            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404WithBody");
                var response = client.Execute(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

		void UrlToStatusCodeHandler(HttpListenerContext obj)
		{
			obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
		}

        [Fact]
        public void Handles_Different_Root_Element_On_Http_Error()
        {
            const string baseUrl = "http://localhost:8080/";
            using(SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("error");
                request.RootElement = "Success";
                request.OnBeforeDeserialization = resp =>
                {
                    if(resp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        request.RootElement = "Error";
                    }
                };

                var response = client.Execute<Response>(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("Not found!", response.Data.Message);
            }
        }

		[Fact]
		public void Handles_Default_Root_Element_On_No_Error()
		{
			const string baseUrl = "http://localhost:8080/";
			using(SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("success");
				request.RootElement = "Success";
				request.OnBeforeDeserialization = resp =>
				{
					if(resp.StatusCode == HttpStatusCode.NotFound)
					{
						request.RootElement = "Error";
					}
				};

				var response = client.Execute<Response>(request);

				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				Assert.Equal("Works!", response.Data.Message);
			}
		}
	}

	public class ResponseHandler
	{
		void error(HttpListenerContext context)
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

        void errorwithbody(HttpListenerContext context)
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


		void success(HttpListenerContext context)
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