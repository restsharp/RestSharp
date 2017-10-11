using System;
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
            Assert.AreEqual(stringWithLimitLength.UrlEncode().Length, numLessThanLimit);
        }

        [Test]
        public void UrlEncode_Returns_Correct_Length_When_More_Than_Limit()
        {
            const int numGreaterThanLimit = 65000;
            var stringWithLimitLength = new string('*', numGreaterThanLimit);
            Assert.AreEqual(stringWithLimitLength.UrlEncode().Length, numGreaterThanLimit);
        }
    }
}