using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace RestSharp.Tests
{
    public class RestRequestTests
    {
        public RestRequestTests()
        {
            CultureChange.SetCurrentCulture(CultureInfo.InvariantCulture);
        }

        [Fact]
        public void Can_Add_Object_With_IntegerArray_property()
        {
            RestRequest request = new RestRequest();

            request.AddObject(new { Items = new [] { 2, 3, 4 } });
        }

        [Fact]
        public void Cannot_Set_Empty_Host_Header()
        {
            RestRequest request = new RestRequest();
            ArgumentException exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", string.Empty));

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
            RestRequest request = new RestRequest();
            ArgumentException exception = Assert.Throws<ArgumentException>(() => request.AddHeader("Host", value));

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
            RestRequest request = new RestRequest();
            request.AddHeader("Host", value);
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData("1", "1")]
        [InlineData("entity", "entity")]
        public void Can_Add_Object_To_UrlSegment(object value, string expectedValue)
        {
            const string ParameterName = "Id";
            RestRequest request = new RestRequest();
            request.AddUrlSegment(ParameterName, value);

            var parameter = request.Parameters.FirstOrDefault(x => x.Name.Equals(ParameterName));
            Assert.NotNull(parameter);
            Assert.Equal(expectedValue, parameter.Value.ToString());
            Assert.Equal(ParameterType.UrlSegment, parameter.Type);
        }
    }
}
