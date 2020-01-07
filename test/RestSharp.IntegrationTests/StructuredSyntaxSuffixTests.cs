using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp.Serialization.Json;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class StructuredSyntaxSuffixTests
    {
        TestHttpServer _server;
        string         _url;

        class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        const string XmlContent  = "<Person><name>Bob</name><age>50</age></Person>";
        const string JsonContent = @"{ ""name"":""Bob"", ""age"":50 }";

        [SetUp]
        public void Setup()
        {
            _server = new TestHttpServer(0, "", HandleRequest);
            _url    = $"http://localhost:{_server.Port}";

            static void HandleRequest(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> p)
            {
                response.ContentType = request.QueryString["ct"];
                response.OutputStream.WriteStringUtf8(request.QueryString["c"]);
                response.StatusCode = 200;
            }
        }

        [TearDown]
        public void Teardown() => _server.Dispose();

        [Test]
        public void By_default_application_json_content_type_should_deserialize_as_JSON()
        {
            var client = new RestClient(_url);

            var request = new RestRequest()
                .AddParameter("ct", "application/json")
                .AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_content_types_with_JSON_structured_syntax_suffix_should_deserialize_as_JSON()
        {
            var client = new RestClient(_url);

            var request = new RestRequest()
                .AddParameter("ct", "application/vnd.somebody.something+json")
                .AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_content_types_with_XML_structured_syntax_suffix_should_deserialize_as_XML()
        {
            var client = new RestClient(_url);

            var request = new RestRequest()
                .AddParameter("ct", "application/vnd.somebody.something+xml")
                .AddParameter("c", XmlContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void By_default_text_xml_content_type_should_deserialize_as_XML()
        {
            var client = new RestClient(_url);

            var request = new RestRequest()
                .AddParameter("ct", "text/xml")
                .AddParameter("c", XmlContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void Content_type_that_matches_the_structured_syntax_suffix_format_but_was_given_an_explicit_handler_should_use_supplied_deserializer()
        {
            var client = new RestClient(_url);

            // In spite of the content type (+xml), treat this specific content type as JSON
            client.AddHandler("application/vnd.somebody.something+xml", new JsonSerializer());

            var request = new RestRequest()
                .AddParameter("ct", "application/vnd.somebody.something+xml")
                .AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }

        [Test]
        public void Should_allow_wildcard_content_types_to_be_defined()
        {
            var client = new RestClient(_url);

            // In spite of the content type, handle ALL structured syntax suffixes of "+xml" as JSON
            client.AddHandler("*+xml", new JsonSerializer());

            var request = new RestRequest()
                .AddParameter("ct", "application/vnd.somebody.something+xml")
                .AddParameter("c", JsonContent);

            var response = client.Execute<Person>(request);

            Assert.AreEqual("Bob", response.Data.Name);
            Assert.AreEqual(50, response.Data.Age);
        }
    }
}