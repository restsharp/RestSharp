using System.Globalization;
using NUnit.Framework;
using RestSharp.Extensions;

namespace RestSharp.Tests
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        [TestCase("this_is_a_test", true, "ThisIsATest")]
        [TestCase("this_is_a_test", false, "This_Is_A_Test")]
        public void ToPascalCase(string start, bool removeUnderscores, string finish)
        {
            string result = start.ToPascalCase(removeUnderscores, CultureInfo.InvariantCulture);

            Assert.AreEqual(finish, result);
        }
    }
}
