using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RestSharp.Tests
{
	/// <summary>
	/// Note: These tests do not handle QueryString building, which is handled in Http, not RestClient
	/// </summary>
	public class UrlBuilderTests
	{
		[Fact]
		public void GET_with_leading_slash()
		{
			var request = new RestRequest("/resource");
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_leading_slash()
		{
			var request = new RestRequest("/resource", Method.POST);
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_leading_slash_and_baseurl_trailing_slash()
		{
			var request = new RestRequest("/resource");
			request.AddParameter("foo", "bar");
			var client = new RestClient("http://example.com/");

			var expected = new Uri("http://example.com/resource?foo=bar");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_leading_slash_and_baseurl_trailing_slash()
		{
			var request = new RestRequest("/resource", Method.POST);
			var client = new RestClient("http://example.com/");

			var expected = new Uri("http://example.com/resource");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_resource_containing_slashes()
		{
			var request = new RestRequest("resource/foo");
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource/foo");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_resource_containing_slashes()
		{
			var request = new RestRequest("resource/foo", Method.POST);
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource/foo");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_resource_containing_tokens()
		{
			var request = new RestRequest("resource/{foo}");
			request.AddUrlSegment("foo", "bar");
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource/bar");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_resource_containing_tokens()
		{
			var request = new RestRequest("resource/{foo}", Method.POST);
			request.AddUrlSegment("foo", "bar");
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/resource/bar");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_empty_request()
		{
			var request = new RestRequest();
			var client = new RestClient("http://example.com/resource");

			var expected = new Uri("http://example.com/resource");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_empty_request_and_bare_hostname()
		{
			var request = new RestRequest();
			var client = new RestClient("http://example.com");

			var expected = new Uri("http://example.com/");
			var output = client.BuildUri(request);

			Assert.Equal(expected, output);
		}

	}
}
