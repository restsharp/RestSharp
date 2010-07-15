using System;
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

		//[Fact]
		//public void Can_Obtain_OAuth_Request_Token()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret");
		//    var request = new RestRequest("request_token.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("oauth_token=requestkey&oauth_token_secret=requestsecret", response.Content);
		//}

		//[Fact]
		//public void Can_Obtain_OAuth_Access_Token()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "requestkey", "requestsecret");
		//    var request = new RestRequest("access_token.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("oauth_token=accesskey&oauth_token_secret=accesssecret", response.Content);

		//}

		//[Fact]
		//public void Can_Make_Authenticated_OAuth_Call_With_Parameters()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");
		//    var request = new RestRequest("echo_api.php");
		//    request.AddParameter("foo", "bar");
		//    request.AddParameter("fizz", "pop");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("fizz=pop&foo=bar", response.Content);
		//}

		//[Fact]
		//public void Can_Make_Authenticated_OAuth_Call()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");
		//    var request = new RestRequest("echo_api.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal(string.Empty, response.Content);

		//}

	}
}
