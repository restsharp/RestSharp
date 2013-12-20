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
    [Trait("Unit", "URL Builder Tests")]
	public class UrlBuilderTests
	{
		[Fact]
		public void GET_with_leading_slash()
		{            
            var request = new RestRequest("/resource");
			var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_leading_slash()
		{            
            var request = new RestRequest("/resource", Method.POST);
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource");
			Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_leading_slash_and_baseurl_trailing_slash()
		{
			var request = new RestRequest("/resource");
			request.AddParameter("foo", "bar");
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource?foo=bar");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_wth_trailing_slash_and_query_parameters()
		{
			var request = new RestRequest("/resource/");
			request.AddParameter("foo", "bar");
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource/?foo=bar");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_leading_slash_and_baseurl_trailing_slash()
		{
			var request = new RestRequest("/resource", Method.POST);
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_resource_containing_slashes()
		{
			var request = new RestRequest("resource/foo");
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource/foo");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_resource_containing_slashes()
		{
			var request = new RestRequest("resource/foo", Method.POST);
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource/foo");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_resource_containing_tokens()
		{
			var request = new RestRequest("resource/{foo}");
			request.AddUrlSegment("foo", "bar");
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource/bar");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_resource_containing_tokens()
		{
			var request = new RestRequest("resource/{foo}", Method.POST);
			request.AddUrlSegment("foo", "bar");
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource/bar");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_empty_request()
		{
			var request = new RestRequest();
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void GET_with_empty_request_and_bare_hostname()
		{
			var request = new RestRequest();
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/");
            Assert.Equal(expected, output);
		}

		[Fact]
		public void POST_with_querystring_containing_tokens()
		{
			var request = new RestRequest("resource", Method.POST);
			request.AddParameter("foo", "bar", ParameterType.QueryString);
            var output = UriBuilder.Build("http://example.com", request);

            var expected = new Uri("http://example.com/resource?foo=bar");
            Assert.Equal(expected, output);
		}

        [Fact]
        public void GET_with_multiple_instances_of_same_key()
        {
            var request = new RestRequest("v1/people/~/network/updates", Method.GET);
            request.AddParameter("type", "STAT");
            request.AddParameter("type", "PICT");
            request.AddParameter("count", "50");
            request.AddParameter("start", "50");
            var output = UriBuilder.Build("http://api.linkedin.com", request);

            var expected = new Uri("http://api.linkedin.com/v1/people/~/network/updates?type=STAT&type=PICT&count=50&start=50");
            Assert.Equal(expected, output);
        }
	}
}
