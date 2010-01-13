using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Net;

namespace RestSharp.WebTests
{
	public class StatusCodeTests
	{
		[Fact]
		public void Can_Handle_404() {
			var client = new RestClient();

			var request = new RestRequest();
			request.BaseUrl = "http://localhost:56976";
			request.Action = "StatusCode/404";

			var response = client.Execute(request);

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public void Can_Handle_Nonexisting_Url_EndPoint() {
			var client = new RestClient();

			var request = new RestRequest();
			request.BaseUrl = "http://nonexistantdomainimguessing.org";
			request.Action = "foo";

			var response = client.Execute(request);

			Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
		}
	}
}