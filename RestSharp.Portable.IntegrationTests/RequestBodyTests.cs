using System.IO;
using System.Net;
using RestSharp.IntegrationTests.Helpers;
//using Xunit;
using NUnit;
using NUnit.Framework;

namespace RestSharp.IntegrationTests
{
    //[Trait("Integration", "Request Body Tests")]
    [Category("Integation [Request Body Tests]")]
	public class RequestBodyTests
	{
		private const string baseUrl = "http://localhost:8080/";

		//[Fact]
        [Test]
		public async void Can_Not_Be_Added_To_GET_Request()
		{
			const Method httpMethod = Method.GET;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasNoRequestBody();
			}
		}

        //[Fact]
        [Test]
		public async void Can_Be_Added_To_POST_Request()
		{
			const Method httpMethod = Method.POST;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasRequestBody(contentType, bodyData);
			}
		}

        //[Fact]
		[Test]
		public async void Can_Be_Added_To_PUT_Request()
		{
			const Method httpMethod = Method.PUT;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasRequestBody(contentType, bodyData);
			}
		}

        //[Fact]
        [Test]
		public async void Can_Be_Added_To_DELETE_Request()
		{
			const Method httpMethod = Method.DELETE;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasRequestBody(contentType, bodyData);
			}
		}

        //[Fact]
        [Test]
		public async void Can_Not_Be_Added_To_HEAD_Request()
		{
			const Method httpMethod = Method.HEAD;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasNoRequestBody();
			}
		}

        //[Fact]
        [Test]
		public async void Can_Be_Added_To_OPTIONS_Request()
		{
			const Method httpMethod = Method.OPTIONS;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasRequestBody(contentType, bodyData);
			}
		}

        //[Fact]
        [Test]
		public async void Can_Be_Added_To_PATCH_Request()
		{
			const Method httpMethod = Method.PATCH;
			using (SimpleServer.Create(baseUrl, Handlers.Generic<RequestBodyCapturer>()))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest(RequestBodyCapturer.RESOURCE, httpMethod);

				const string contentType = "text/plain";
				const string bodyData = "abc123 foo bar baz BING!";
				request.AddParameter(contentType, bodyData, ParameterType.RequestBody);

				await client.ExecuteAsync(request);

				AssertHasRequestBody(contentType, bodyData);
			}
		}

		private static void AssertHasNoRequestBody()
		{
			Assert.Null(RequestBodyCapturer.CapturedContentType);
			Assert.AreEqual(false, RequestBodyCapturer.CapturedHasEntityBody);
			Assert.AreEqual(string.Empty, RequestBodyCapturer.CapturedEntityBody);
		}

		private static void AssertHasRequestBody(string contentType, string bodyData)
		{
			Assert.AreEqual(contentType, RequestBodyCapturer.CapturedContentType);
			Assert.AreEqual(true, RequestBodyCapturer.CapturedHasEntityBody);
			Assert.AreEqual(bodyData, RequestBodyCapturer.CapturedEntityBody);
		}

		private class RequestBodyCapturer
		{
			public const string RESOURCE = "Capture";

			public static string CapturedContentType { get; set; }
			public static bool CapturedHasEntityBody { get; set; }
			public static string CapturedEntityBody { get; set; }

			public static void Capture(HttpListenerContext context)
			{
				var request = context.Request;
				CapturedContentType = request.ContentType;
				CapturedHasEntityBody = request.HasEntityBody;
				CapturedEntityBody = StreamToString(request.InputStream);
			}

			private static string StreamToString(Stream stream)
			{
				var streamReader = new StreamReader(stream);
				return streamReader.ReadToEnd();
			}
		}
	}
}
