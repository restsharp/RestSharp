using System;
using NUnit.Framework;
using System.Text;

namespace RestSharp.Tests
{
    /// <summary>
    /// Note: These tests do not handle QueryString building, which is handled in Http, not RestClient
    /// </summary>
    [TestFixture]
    public class UrlBuilderTests
    {
        [Test]
        public void Should_not_duplicate_question_mark()
        {
            RestRequest request = new RestRequest();

            request.AddParameter("param2", "value2");

            RestClient client = new RestClient("http://example.com/resource?param1=value1");
            Uri expected = new Uri("http://example.com/resource?param1=value1&param2=value2");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_leading_slash()
        {
            RestRequest request = new RestRequest("/resource");
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void POST_with_leading_slash()
        {
            RestRequest request = new RestRequest("/resource", Method.POST);
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_leading_slash_and_baseurl_trailing_slash()
        {
            RestRequest request = new RestRequest("/resource");

            request.AddParameter("foo", "bar");

            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource?foo=bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_wth_trailing_slash_and_query_parameters()
        {
            RestRequest request = new RestRequest("/resource/");
            RestClient client = new RestClient("http://example.com");

            request.AddParameter("foo", "bar");

            Uri expected = new Uri("http://example.com/resource/?foo=bar");
            Uri output = client.BuildUri(request);

            client.Execute(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_empty_request_and_query_parameters_without_encoding()
        {
            RestRequest request = new RestRequest();

            request.AddQueryParameter("foo", "bar,baz", false);

            RestClient client = new RestClient("http://example.com/resource?param1=value1");
            Uri expected = new Uri("http://example.com/resource?param1=value1&foo=bar,baz");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void POST_with_leading_slash_and_baseurl_trailing_slash()
        {
            RestRequest request = new RestRequest("/resource", Method.POST);
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_resource_containing_slashes()
        {
            RestRequest request = new RestRequest("resource/foo");
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource/foo");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void POST_with_resource_containing_slashes()
        {
            RestRequest request = new RestRequest("resource/foo", Method.POST);
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource/foo");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_resource_containing_tokens()
        {
            RestRequest request = new RestRequest("resource/{foo}");

            request.AddUrlSegment("foo", "bar");

            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource/bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_resource_containing_null_token()
        {
            RestRequest request = new RestRequest("/resource/{foo}", Method.GET);

            request.AddUrlSegment("foo", null);

            RestClient client = new RestClient("http://example.com/api/1.0");
            ArgumentException exception = Assert.Throws<ArgumentException>(() => client.BuildUri(request));

            Assert.IsNotNull(exception);
            Assert.False(string.IsNullOrEmpty(exception.Message));
            Assert.IsTrue(exception.Message.Contains("foo"));
        }

        [Test]
        public void POST_with_resource_containing_tokens()
        {
            RestRequest request = new RestRequest("resource/{foo}", Method.POST);

            request.AddUrlSegment("foo", "bar");

            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/resource/bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_empty_request()
        {
            RestRequest request = new RestRequest();
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_empty_request_and_bare_hostname()
        {
            RestRequest request = new RestRequest();
            RestClient client = new RestClient(new Uri("http://example.com"));
            Uri expected = new Uri("http://example.com/");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void POST_with_querystring_containing_tokens()
        {
            RestRequest request = new RestRequest("resource", Method.POST);

            request.AddParameter("foo", "bar", ParameterType.QueryString);

            RestClient client = new RestClient("http://example.com");
            Uri expected = new Uri("http://example.com/resource?foo=bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_multiple_instances_of_same_key()
        {
            RestRequest request = new RestRequest("v1/people/~/network/updates", Method.GET);

            request.AddParameter("type", "STAT");
            request.AddParameter("type", "PICT");
            request.AddParameter("count", "50");
            request.AddParameter("start", "50");

            RestClient client = new RestClient("http://api.linkedin.com");
            Uri expected = new Uri("http://api.linkedin.com/v1/people/~/network/updates?type=STAT&type=PICT&count=50&start=50");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_Uri_containing_tokens()
        {
            RestRequest request = new RestRequest();

            request.AddUrlSegment("foo", "bar");

            RestClient client = new RestClient(new Uri("http://example.com/{foo}"));
            Uri expected = new Uri("http://example.com/bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_Url_string_containing_tokens()
        {
            RestRequest request = new RestRequest();

            request.AddUrlSegment("foo", "bar");

            RestClient client = new RestClient("http://example.com/{foo}");
            Uri expected = new Uri("http://example.com/bar");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_Uri_and_resource_containing_tokens()
        {
            RestRequest request = new RestRequest("resource/{baz}");

            request.AddUrlSegment("foo", "bar");
            request.AddUrlSegment("baz", "bat");

            RestClient client = new RestClient(new Uri("http://example.com/{foo}"));
            Uri expected = new Uri("http://example.com/bar/resource/bat");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_Url_string_and_resource_containing_tokens()
        {
            RestRequest request = new RestRequest("resource/{baz}");

            request.AddUrlSegment("foo", "bar");
            request.AddUrlSegment("baz", "bat");

            RestClient client = new RestClient("http://example.com/{foo}");
            Uri expected = new Uri("http://example.com/bar/resource/bat");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void GET_with_Invalid_Url_string_throws_exception()
        {
            Assert.Throws<UriFormatException>(delegate { new RestClient("invalid url"); });
        }

        [Test]
        public void Should_add_parameter_if_it_is_new()
        {
            RestRequest request = new RestRequest();

            request.AddOrUpdateParameter("param2", "value2");
            request.AddOrUpdateParameter("param3", "value3");

            RestClient client = new RestClient("http://example.com/resource?param1=value1");
            Uri expected = new Uri("http://example.com/resource?param1=value1&param2=value2&param3=value3");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void Should_update_parameter_if_it_already_exists()
        {
            RestRequest request = new RestRequest();

            request.AddOrUpdateParameter("param2", "value2");
            request.AddOrUpdateParameter("param2", "value2-1");

            RestClient client = new RestClient("http://example.com/resource?param1=value1");
            Uri expected = new Uri("http://example.com/resource?param1=value1&param2=value2-1");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void Should_encode_colon()
        {
            RestRequest request = new RestRequest();
            // adding parameter with o-slash character which is encoded differently between
            // utf-8 and iso-8859-1
            request.AddOrUpdateParameter("parameter", "some:value");

            RestClient client = new RestClient("http://example.com/resource");

            Uri expectedDefaultEncoding = new Uri("http://example.com/resource?parameter=some%3avalue");
            Assert.AreEqual(expectedDefaultEncoding, client.BuildUri(request));
        }
      
        [Test]
        public void Should_build_uri_using_selected_encoding()
        {
            RestRequest request = new RestRequest();
            // adding parameter with o-slash character which is encoded differently between
            // utf-8 and iso-8859-1
            request.AddOrUpdateParameter("town", "Hillerød");

            RestClient client = new RestClient("http://example.com/resource");

            Uri expectedDefaultEncoding = new Uri("http://example.com/resource?town=Hiller%C3%B8d");
            Uri expectedIso89591Encoding = new Uri("http://example.com/resource?town=Hiller%F8d");
            Assert.AreEqual(expectedDefaultEncoding, client.BuildUri(request));
            // now changing encoding
            client.Encoding = Encoding.GetEncoding("ISO-8859-1");
            Assert.AreEqual(expectedIso89591Encoding, client.BuildUri(request));
        }
      
        [Test]
        public void Should_build_uri_with_resource_full_uri()
        {
            RestRequest request = new RestRequest("https://www.example1.com/connect/authorize");
            
            RestClient client = new RestClient("https://www.example1.com/");
            Uri expected = new Uri("https://www.example1.com/connect/authorize");
            Uri output = client.BuildUri(request);

            Assert.AreEqual(expected, output);
        }

    }
}
