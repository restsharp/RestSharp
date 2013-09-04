using System;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class MultipartFormDataTests
	{
		[Fact]
		public void MultipartFormDataAsync() {
			const string baseUrl = "http://localhost:8080/";
			using (SimpleServer.Create(baseUrl, EchoHandler)) {
				var client = new RestClient(baseUrl);
				var request = new RestRequest("/");
				request.Method = Method.POST;
				request.AlwaysMultipartFormData = true;
				AddParameters(request);
				var restRequestAsyncHandle = client.ExecuteAsync(request, (restResponse, handle) => {
					Console.WriteLine(restResponse.Content);
					Assert.True(restResponse.Content == Expected);
				});
			}
		}

		[Fact]
		public void MultipartFormData() {
			//const string baseUrl = "http://localhost:8080/";
			const string baseUrl = "http://localhost:8080/";
			using (SimpleServer.Create(baseUrl, EchoHandler)) {
				var client = new RestClient(baseUrl);
				var request = new RestRequest("/");
				request.Method = Method.POST;
				request.AlwaysMultipartFormData = true;
				AddParameters(request);
				var response = client.Execute(request);
				Console.WriteLine(response.Content);

				Assert.True(response.Content == Expected);
			}
		}

		private void AddParameters(RestRequest request) {
			request.AddParameter("foo", "bar");
			request.AddParameter("a name with spaces", "somedata");
		}
		private const string Expected = @"-------------------------------28947758029299
Content-Disposition: form-data; name=""foo""

bar
-------------------------------28947758029299
Content-Disposition: form-data; name=""a name with spaces""

somedata
-------------------------------28947758029299--
";

		private void EchoHandler(HttpListenerContext obj) {
			obj.Response.StatusCode = 200;
			var streamReader = new System.IO.StreamReader(obj.Request.InputStream);
			obj.Response.OutputStream.WriteStringUtf8(streamReader.ReadToEnd());
		}
	}
}
