using System;
using NUnit.Framework;

namespace RestSharp.Tests
{
    public class HostHeaderTests
    {
        [Test]
        public void Cannot_Set_Empty_Host_Header()
        {
            var request   = new RestRequest();
            var exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", string.Empty));

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
            var request   = new RestRequest();
            var exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", value));

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
            var request = new RestRequest();

            Assert.DoesNotThrow(() => request.AddHeader("Host", value));
        }
    }
}