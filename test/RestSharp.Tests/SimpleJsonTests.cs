using System.Linq;
using NUnit.Framework;

namespace RestSharp.Tests
{
    [TestFixture]
    public class SimpleJsonTests
    {
        [Test]
        public void EscapeToJavascriptString_should_not_double_escape()
        {
            var preformattedString = "{ \"name\" : \"value\" }";
            var expectedSlashCount = preformattedString.Count(x => x == '\\');

            var result           = SimpleJson.EscapeToJavascriptString(preformattedString);
            var actualSlashCount = result.Count(x => x == '\\');

            Assert.AreEqual(expectedSlashCount, actualSlashCount);
        }

        [Test]
        public void SerializeObject_should_not_assume_strings_wrapped_in_curly_braces_are_json()
        {
            var objectWithCurlyString = new {Name = "{value}"};

            var result = SimpleJson.SerializeObject(objectWithCurlyString);

            Assert.AreEqual("{\"Name\":\"{value}\"}", result);
        }
    }
}