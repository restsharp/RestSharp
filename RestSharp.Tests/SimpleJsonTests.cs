using NUnit.Framework;
using System.Linq;

namespace RestSharp.Tests
{
    [TestFixture]
    public class SimpleJsonTests
    {
        [Test]
        public void SerializeObject_should_not_assume_strings_wrapped_in_curly_braces_are_json()
        {
            var objectWithCurlyString = new { Name = "{value}" };

            string result = SimpleJson.SimpleJson.SerializeObject(objectWithCurlyString);

            Assert.AreEqual("{\"Name\":\"{value}\"}", result);
        }

        [Test]
        public void EscapeToJavascriptString_should_not_double_escape()
        {
            string preformattedString = "{ \"name\" : \"value\" }";
            int expectedSlashCount = preformattedString.Count(x => x == '\\');

            string result = SimpleJson.SimpleJson.EscapeToJavascriptString(preformattedString);
            int actualSlashCount = result.Count(x => x == '\\');

            Assert.AreEqual(expectedSlashCount, actualSlashCount);
        }
    }
}
