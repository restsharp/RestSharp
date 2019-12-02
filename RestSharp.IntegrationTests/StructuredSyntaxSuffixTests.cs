using System.Net;
using NUnit.Framework;
using RestSharp.IntegrationTests.Helpers;
using RestSharp.Serialization.Json;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StructuredSyntaxSuffixTests
    {
        SimpleServer _server;

        class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        const string XmlContent  = "<Person><name>Bob</name><age>50</age></Person>";
        const string JsonContent = @"{ ""name"":""Bob"", ""age"":50 }";

        static void QueryStringBasedContentAndContentTypeHandler(HttpListenerContext obj)
        {
            obj.Response.ContentType = obj.Request.QueryString["ct"];
            obj.Response.OutputStream.WriteStringUtf8(obj.Request.QueryString["c"]);
            obj.Response.StatusCode = 200;
        }

        [OneTimeSetUp]
        public void Setup() => _server = SimpleServer.Create(QueryStringBasedContentAndContentTypeHandler);

        [OneTimeTearDown]
        public void Teardown() => _server.Dispose();

        [Test]
        public void By_default_application_json_content_type_should_deserialize_as_JSON()
        {
            var client  = new RestClient(_server.Url);
            var request = new RestRequest();

            request.AddParameter("ct", "application/json");
            request.AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON()
        {
            var client  = new RestClient(_server.Url);
            var request = new RestRequest();

            request.AddParameter("ct", "application/vnd.somebody.something+json");
            request.AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML()
        {
            var client  = new RestClient(_server.Url);
            var request = new RestRequest();

            request.AddParameter("ct", "application/vnd.somebody.something+xml");
            request.AddParameter("c", XmlContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_text_xml_content_type_should_deserialize_as_XML()
        {
            var client  = new RestClient(_server.Url);
            var request = new RestRequest();

            request.AddParameter("ct", "text/xml");
            request.AddParameter("c", XmlContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void Content_type_that_matches_the_structured_syntax_suffix_format_but_was_given_an_explicit_handler_should_use_supplied_deserializer()
        {
            var client = new RestClient(_server.Url);

            // In spite of the content type (+xml), treat this specific content type as JSON
            client.AddHandler("application/vnd.somebody.something+xml", new JsonSerializer());

            var request = new RestRequest();

            request.AddParameter("ct", "application/vnd.somebody.something+xml");
            request.AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void Should_allow_wildcard_content_types_to_be_defined()
        {
            var client = new RestClient(_server.Url);

            // In spite of the content type, handle ALL structured syntax suffixes of "+xml" as JSON
            client.AddHandler("*+xml", new JsonSerializer());

            var request = new RestRequest();

            request.AddParameter("ct", "application/vnd.somebody.something+xml");
            request.AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }
    }
}