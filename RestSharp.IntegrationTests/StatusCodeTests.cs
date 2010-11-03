using Xunit;
using System.Net;

namespace RestSharp.IntegrationTests
{
	public class StatusCodeTests : TestBase
	{
		[Fact]
		public void Handles_GET_Request_404_Error()
		{
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("StatusCode/404");
			var response = client.Execute(request);
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public void Handles_Non_Existent_Domain()
		{
			var client = new RestClient("http://nonexistantdomainimguessing.org");
			var request = new RestRequest("foo");
			var response = client.Execute(request);
			Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
		}

		[Fact]
		public void Handles_Different_Root_Element_On_Error()
		{
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("ErrorHandling/NotFound");
			request.RootElement = "Success";
			request.ErrorRootElement = "Error";
			request.ErrorCondition = resp =>
			{
				return resp.StatusCode == HttpStatusCode.BadRequest;
			};

			var response = client.Execute<Response>(request);

			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Equal("Not found!", response.Data.Message);
		}

		[Fact]
		public void Handles_Default_Root_Element_On_No_Error()
		{
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("ErrorHandling/Success");
			request.RootElement = "Success";
			request.ErrorRootElement = "Error";
			request.ErrorCondition = resp =>
			{
				return resp.StatusCode == HttpStatusCode.NotFound;
			};

			var response = client.Execute<Response>(request);

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("Works!", response.Data.Message);
		}

	}

	public class Response
	{
		public string Message { get; set; }
	}
}