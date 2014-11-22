using System;
using System.Globalization;
using Xunit;
using Xunit.Extensions;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Authenticators.OAuth;

namespace RestSharp.Tests
{
    public class OAuthTests
    {
        public OAuthTests()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        }

        [Theory]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [InlineData("0123456789", "0123456789")]
        [InlineData("-._~", "-._~")]
        [InlineData(" !\"#$%&'()*+,", "%20%21%22%23%24%25%26%27%28%29%2A%2B%2C")]
        [InlineData("%$%", "%25%24%25")]
        [InlineData("%", "%25")]
        [InlineData("/:;<=>?@", "%2F%3A%3B%3C%3D%3E%3F%40")]
        [InlineData("\x00\x01\a\b\f\n\r\t\v", @"%00%01%07%08%0C%0A%0D%09%0B")]
        public void UrlStrictEncode_Encodes_Correctly(string value, string expected)
        {
            var actual = OAuthTools.UrlEncodeStrict(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1234", "%31%32%33%34")]
        [InlineData("\x00\x01\x02\x03", "%00%01%02%03")]
        [InlineData("\r\n\t", "%0D%0A%09")]
        public void PercentEncode_Encodes_Correctly(string value, string expected)
        {
            var actual = value.PercentEncode();
            Assert.Equal(expected, actual);
        }
    }
}
