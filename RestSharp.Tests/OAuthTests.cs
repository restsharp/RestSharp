using System.Globalization;
using System.Threading;
using NUnit.Framework;
using RestSharp.Authenticators.OAuth;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp.Tests
{
    [TestFixture]
    public class OAuthTests
    {
        public OAuthTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        }

        [Test]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("0123456789", "0123456789")]
        [TestCase("-._~", "-._~")]
        [TestCase(" !\"#$%&'()*+,", "%20%21%22%23%24%25%26%27%28%29%2A%2B%2C")]
        [TestCase("%$%", "%25%24%25")]
        [TestCase("%", "%25")]
        [TestCase("/:;<=>?@", "%2F%3A%3B%3C%3D%3E%3F%40")]
        [TestCase("\x00\x01\a\b\f\n\r\t\v", @"%00%01%07%08%0C%0A%0D%09%0B")]
        public void UrlStrictEncode_Encodes_Correctly(string value, string expected)
        {
            string actual = OAuthTools.UrlEncodeStrict(value);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("1234", "%31%32%33%34")]
        [TestCase("\x00\x01\x02\x03", "%00%01%02%03")]
        [TestCase("\r\n\t", "%0D%0A%09")]
        public void PercentEncode_Encodes_Correctly(string value, string expected)
        {
            string actual = value.PercentEncode();

            Assert.AreEqual(expected, actual);
        }
    }
}
