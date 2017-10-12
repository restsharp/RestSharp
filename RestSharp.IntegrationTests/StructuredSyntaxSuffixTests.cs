using System;
using System.Net;
using NUnit.Framework;
using RestSharp.Deserializers;
using RestSharp.IntegrationTests.Helpers;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StructuredSyntaxSuffixTests
    {
        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private const string XML_CONTENT = "<Person><name>Bob</name><age>50</age></Person>";

        private const string JSON_CONTENT = @"{ ""name"":""Bob"", ""age"":50 }";

        private static void QueryStringBasedContentAndContentTypeHandler(HttpListenerContext obj)
        {
            obj.Response.ContentType = obj.Request.QueryString["ct"];
            obj.Response.OutputStream.WriteStringUtf8(obj.Request.QueryString["c"]);
            obj.Response.StatusCode = 200;
        }

        [Test]
        public void By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest();

                request.AddParameter("ct", "application/vnd.somebody.something+json");
                request.AddParameter("c", JSON_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }

        [Test]
        public void By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest();

                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", XML_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }

        [Test]
        public void Content_type_that_matches_the_structured_syntax_suffix_format_but_was_given_an_explicit_handler_should_use_supplied_deserializer()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);

                // In spite of the content type (+xml), treat this specific content type as JSON
                client.AddHandler("application/vnd.somebody.something+xml", new JsonDeserializer());

                RestRequest request = new RestRequest();

                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", JSON_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }

        [Test]
        public void Should_allow_wildcard_content_types_to_be_defined()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);

                // In spite of the content type, handle ALL structured syntax suffixes of "+xml" as JSON
                client.AddHandler("*+xml", new JsonDeserializer());

                RestRequest request = new RestRequest();

                request.AddParameter("ct", "application/vnd.somebody.something+xml");
                request.AddParameter("c", JSON_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }

        [Test]
        public void By_default_application_json_content_type_should_deserialize_as_JSON()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest();

                request.AddParameter("ct", "application/json");
                request.AddParameter("c", JSON_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }

        [Test]
        public void By_default_text_xml_content_type_should_deserialize_as_XML()
        {
            Uri baseUrl = new Uri("http://localhost:8080/");

            using (SimpleServer.Create(baseUrl.AbsoluteUri, QueryStringBasedContentAndContentTypeHandler))
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest();

                request.AddParameter("ct", "text/xml");
                request.AddParameter("c", XML_CONTENT);

                IRestResponse<Person> response = client.Execute<Person>(request);

                Assert.AreEqual("Bob", response.Data.Name);
                Assert.AreEqual(50, response.Data.Age);
            }
        }
    }
}
