using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class AuthenticationTests : TestBase
	{
		[Fact]
		public void Can_Authenticate_With_Basic_Http_Auth() {
			var client = new RestClient(BaseUrl);
			client.Authenticator = new HttpBasicAuthenticator("testuser", "testpassword");
			var request = new RestRequest("Authentication/Basic");
			var response = client.Execute(request);

			Assert.Equal("testuser|testpassword", response.Content);
		}
	}
}
