using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace RestSharp.Tests
{
    public class RestRequestTests
    {
        public RestRequestTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        }

        [Test]
        public void Can_Add_Object_With_IntegerArray_property()
        {
            RestRequest request = new RestRequest();

            request.AddObject(new { Items = new [] { 2, 3, 4 } });
        }

        [Test]
        public void Cannot_Set_Empty_Host_Header()
        {
            RestRequest request = new RestRequest();
            ArgumentException exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", string.Empty));

            Assert.AreEqual("value", exception.ParamName);
        }

        [Test]
        [TestCase("http://localhost")]
        [TestCase("hostname 1234")]
        [TestCase("-leading.hyphen.not.allowed")]
        [TestCase("bad:port")]
        [TestCase(" no.leading.white-space")]
        [TestCase("no.trailing.white-space ")]
        [TestCase(".leading.dot.not.allowed")]
        [TestCase("double.dots..not.allowed")]
        [TestCase(".")]
        [TestCase(".:2345")]
        [TestCase(":5678")]
        [TestCase("")]
        [TestCase("foo:bar:baz")]
        public void Cannot_Set_Invalid_Host_Header(string value)
        {
            RestRequest request = new RestRequest();
            ArgumentException exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", value));

            Assert.AreEqual("value", exception.ParamName);
        }

        [Test]
        [TestCase("localhost")]
        [TestCase("localhost:1234")]
        [TestCase("host.local")]
        [TestCase("anotherhost.local:2345")]
        [TestCase("www.w3.org")]
        [TestCase("www.w3.org:3456")]
        [TestCase("8.8.8.8")]
        [TestCase("a.1.b.2")]
        [TestCase("10.20.30.40:1234")]
        [TestCase("0host")]
        [TestCase("hypenated-hostname")]
        [TestCase("multi--hyphens")]
        public void Can_Set_Valid_Host_Header(string value)
        {
            RestRequest request = new RestRequest();

            Assert.DoesNotThrow(() => request.AddHeader("Host", value));
        }

        [Test]
        [TestCase(1, "1")]
        [TestCase("1", "1")]
        [TestCase("entity", "entity")]
        public void Can_Add_Object_To_UrlSegment(object value, string expectedValue)
        {
            const string ParameterName = "Id";
            RestRequest request = new RestRequest();
            request.AddUrlSegment(ParameterName, value);

            var parameter = request.Parameters.FirstOrDefault(x => x.Name.Equals(ParameterName));
            Assert.IsNotNull(parameter);
            Assert.AreEqual(expectedValue, parameter.Value.ToString());
            Assert.AreEqual(ParameterType.UrlSegment, parameter.Type);
        }
    }
}
