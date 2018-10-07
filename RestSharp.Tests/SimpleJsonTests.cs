using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Tests
{
    [TestFixture]
    public class SimpleJsonTests
    {
        [Test]
        public void SerializeObject_should_not_modify_properly_formatted_json_strings()
        {
            string preformattedString = "{ \"name\" : \"value\" }";
            int expectedSlashCount = preformattedString.Count(x => x == '\\');

            string result = SimpleJson.SerializeObject(preformattedString);
            int actualSlashCount = result.Count(x => x == '\\');
            
            Assert.AreEqual(preformattedString, result);
            Assert.AreEqual(expectedSlashCount, actualSlashCount);
        }

        [Test]
        public void EscapeToJavascriptString_should_not_double_escape()
        {
            string preformattedString = "{ \"name\" : \"value\" }";
            int expectedSlashCount = preformattedString.Count(x => x == '\\');

            string result = SimpleJson.EscapeToJavascriptString(preformattedString);
            int actualSlashCount = result.Count(x => x == '\\');

            Assert.AreEqual(expectedSlashCount, actualSlashCount);
        }
    }
}
