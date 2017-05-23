using System.Globalization;
using RestSharp.Extensions;
using Xunit;

namespace RestSharp.Tests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("this_is_a_test", true, "ThisIsATest")]
        [InlineData("this_is_a_test", false, "This_Is_A_Test")]
        public void ToPascalCase(string start, bool removeUnderscores, string finish)
        {
            string result = start.ToPascalCase(removeUnderscores, CultureInfo.InvariantCulture);

            Assert.Equal(finish, result);
        }
    }
}
