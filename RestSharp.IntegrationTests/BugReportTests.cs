using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Net;

namespace RestSharp.IntegrationTests
{
	public class BugReportTests : TestBase
	{
		public BugReportTests()
		{
			
		}

		[Fact]
		public void PUT_with_no_params_returns_411()
		{
			var request = new RestRequest("Dump", Method.PUT);

			var client = new RestClient(BaseUrl);

			var response = client.Execute(request);

			Assert.Equal("0", response.Content);
		}
	}
}
