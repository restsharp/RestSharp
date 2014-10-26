using System;
using System.Globalization;
using Xunit;
using Xunit.Extensions;

namespace RestSharp.Tests
{
    public class RestRequestTests
    {
        public RestRequestTests()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        }

        [Fact]
        public void Can_Add_Object_With_IntegerArray_property()
        {
            var request = new RestRequest();
            request.AddObject(new { Items = new int[] { 2, 3, 4 } });
        }

        [Fact]
        public void Cannot_Set_Empty_Host_Header()
        {
            var request = new RestRequest();
            var exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", string.Empty));

            Assert.Equal("value", exception.ParamName);
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("hostname 1234")]
        [InlineData("-leading.hyphen.not.allowed")]
        [InlineData("bad:port")]
        [InlineData(" no.leading.white-space")]
        [InlineData("no.trailing.white-space ")]
        [InlineData(".leading.dot.not.allowed")]
        [InlineData("double.dots..not.allowed")]
        [InlineData(".")]
        [InlineData(".:2345")]
        [InlineData(":5678")]
        [InlineData("")]
        [InlineData("foo:bar:baz")]
        public void Cannot_Set_Invalid_Host_Header(string value)
        {
            var request = new RestRequest();
            var exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", value));

            Assert.Equal("value", exception.ParamName);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("localhost:1234")]
        [InlineData("host.local")]
        [InlineData("anotherhost.local:2345")]
        [InlineData("www.w3.org")]
        [InlineData("www.w3.org:3456")]
        [InlineData("8.8.8.8")]
        [InlineData("a.1.b.2")]
        [InlineData("10.20.30.40:1234")]
        [InlineData("0host")]
        [InlineData("hypenated-hostname")]
        [InlineData("multi--hyphens")]
        public void Can_Set_Valid_Host_Header(string value)
        {
            var request = new RestRequest();

            Assert.DoesNotThrow(() => request.AddHeader("Host", value));
        }
    }
}
