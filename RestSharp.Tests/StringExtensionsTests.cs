using System;
using System.Text;
using NUnit.Framework;
using RestSharp.Extensions;

namespace RestSharp.Tests
{
    public class StringExtensionsTests
    {
        [Test]
        public void UrlEncode_Throws_ArgumentNullException_For_Null_Input()
        {
            const string nullString = null;
            Assert.Throws<ArgumentNullException>(
                delegate { nullString.UrlEncode(); });
        }

        [Test]
        public void UrlEncode_Returns_Correct_Length_When_Less_Than_Limit()
        {
            const int numLessThanLimit = 32766;
            var stringWithLimitLength = new string('*', numLessThanLimit);
            var encodedAndDecoded = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(numLessThanLimit, encodedAndDecoded.Length);
        }

        [Test]
        public void UrlEncode_Returns_Correct_Length_When_More_Than_Limit()
        {
            const int numGreaterThanLimit = 65000;
            var stringWithLimitLength = new string('*', numGreaterThanLimit);
            var encodedAndDecoded = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(numGreaterThanLimit, encodedAndDecoded.Length);
        }

        [Test]
        public void UrlEncodeTest()
        {
            const string parameter = "ø";
            Assert.True(string.Equals("%F8", parameter.UrlEncode(Encoding.GetEncoding("ISO-8859-1")), StringComparison.OrdinalIgnoreCase));
            Assert.True(string.Equals("%C3%B8", parameter.UrlEncode(), StringComparison.OrdinalIgnoreCase));
        }
    }
}