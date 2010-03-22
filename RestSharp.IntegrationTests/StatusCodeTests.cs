using Xunit;
using System.Net;

namespace RestSharp.IntegrationTests
{
	public class StatusCodeTests : TestBase
	{
		[Fact]
		public void Handles_GET_Request_404_Error() {
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("StatusCode/404");
			var response = client.Execute(request);
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public void Handles_Non_Existent_Domain() {
			var client = new RestClient("http://nonexistantdomainimguessing.org");
			var request = new RestRequest("foo");
			var response = client.Execute(request);
			Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
		}
	}
}