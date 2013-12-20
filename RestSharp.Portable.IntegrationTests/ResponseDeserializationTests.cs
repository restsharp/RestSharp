using System.Linq;
using RestSharp.IntegrationTests.Helpers;
using Xunit;
using System.Net;

namespace RestSharp.IntegrationTests
{
    //[Trait("Integration", "Deserialization Tests")]
	public class DeserializationTests
    {
        const string baseUrl = "http://localhost:8080/";

        [Fact]
        
        public void Handles_Different_Root_Element_On_Http_Error()
        {
            using (SimpleServer.Create(baseUrl, Handlers.Generic<ResponseHandler>()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("error");
                request.RootElement = "Success";
                request.OnBeforeDeserialization = resp =>
                {
                    if (resp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        request.RootElement = "Error";
                    }
                };

                var result = client.ExecuteAsync<Response>(request);
                result.Wait();

                var response = result.Result;

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("Not found!", response.Data.Message);
            }
        }

        [Fact]
        
		public void Handles_Default_Root_Element_On_No_Error()
		{
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

                var result = client.ExecuteAsync<Response>(request);
                result.Wait();
                var response = result.Result;
				
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				Assert.Equal("Works!", response.Data.Message);
			}
		}
	}

	public class ResponseHandler
	{
		public void error(HttpListenerContext context)
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

        public void errorwithbody(HttpListenerContext context)
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
	}

	public class Response
	{
		public string Message { get; set; }
	}
}