using System;
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

        [Test]
        [TestCase("The quick brown fox jumps over the lazy dog", "rVL90tHhGt0eQ0TCITY74nVL22P%2FltlWS7WvJXpECPs%3D")]
        [TestCase("The quick\tbrown\nfox\rjumps\r\nover\t\tthe\n\nlazy\r\n\r\ndog", "C%2B2RY0Hna6VrfK1crCkU%2FV1e0ECoxoDh41iOOdmEMx8%3D")]
        [TestCase("", "%2BnkCwZfv%2FQVmBbNZsPKbBT3kAg3JtVn3f3YMBtV83L8%3D")]
        [TestCase(" !\"#$%&'()*+,", "xcTgWGBVZaw%2Bilg6kjWAGt%2FhCcsVBMMe1CcDEnxnh8Y%3D")]
        public void HmacSha256_Hashes_Correctly(string value, string expected)
        {
            string consumerSecret = "12345678";
            string actual = OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha256, value, consumerSecret);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HmacSha256_Does_Not_Accept_Nulls()
        {
            string consumerSecret = "12345678";
            string actual = OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha256, null, consumerSecret);
        }
    }
}
